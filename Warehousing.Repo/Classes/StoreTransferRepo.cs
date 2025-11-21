using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class StoreTransferRepo : RepositoryBase<StoreTransfer>, IStoreTransferRepo
    {
        private readonly WarehousingContext _context;
        private readonly IMapper _mapper;

        public StoreTransferRepo(WarehousingContext context, ILogger<StoreTransferRepo> logger, IConfiguration config) : base(context, logger, config)
        {
            _context = context;
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()));
        }

        public async Task<IEnumerable<StoreTransferDto>> GetTransfersByStatusAsync(int statusId)
        {
            var transfers = await _context.StoreTransfers
                .Include(t => t.FromStore)
                .Include(t => t.ToStore)
                .Include(t => t.Status)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                .Where(t => t.StatusId == statusId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StoreTransferDto>>(transfers);
        }

        public async Task<IEnumerable<StoreTransferDto>> GetTransfersByStoreAsync(int storeId, bool isFromStore = true)
        {
            var query = _context.StoreTransfers
                .Include(t => t.FromStore)
                .Include(t => t.ToStore)
                .Include(t => t.Status)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            if (isFromStore)
            {
                query = query.Where(t => t.FromStoreId == storeId);
            }
            else
            {
                query = query.Where(t => t.ToStoreId == storeId);
            }

            var transfers = await query.ToListAsync();
            return _mapper.Map<IEnumerable<StoreTransferDto>>(transfers);
        }

        public async Task<StoreTransferDto> GetTransferWithItemsAsync(int transferId)
        {
            var transfer = await _context.StoreTransfers
                .Include(t => t.FromStore)
                .Include(t => t.ToStore)
                .Include(t => t.Status)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new ArgumentException($"Transfer with ID {transferId} not found");

            return _mapper.Map<StoreTransferDto>(transfer);
        }

        public async Task CompleteTransferAsync(int transferId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var transfer = await _context.StoreTransfers
                        .Include(t => t.Items)
                        .FirstOrDefaultAsync(t => t.Id == transferId);

                    if (transfer == null)
                        throw new InvalidOperationException($"Store transfer with ID {transferId} was not found.");

                    if (transfer.Items == null || transfer.Items.Count == 0)
                        throw new InvalidOperationException("Cannot complete a store transfer that has no items.");

                    // Update status to completed (ensure status exists)
                    var completedStatus = await EnsureStatusAsync(
                        "COMPLETED",
                        "مكتمل",
                        "Completed",
                        "Store transfer completed");

                    transfer.StatusId = completedStatus.Id;

                    // Create inventory transactions for each item
                    foreach (var item in transfer.Items)
                    {
                        // Deduction from source store
                        await CreateInventoryTransaction(
                            item.ProductId,
                            transfer.FromStoreId,
                            -item.Quantity,
                            item.UnitCost,
                            "TRANSFER_OUT",
                            transferId);

                        // Addition to destination store
                        await CreateInventoryTransaction(
                            item.ProductId,
                            transfer.ToStoreId,
                            item.Quantity,
                            item.UnitCost,
                            "TRANSFER_IN",
                            transferId);

                        // Update inventory records
                        await UpdateInventory(transfer.FromStoreId, item.ProductId, -item.Quantity);
                        await UpdateInventory(transfer.ToStoreId, item.ProductId, item.Quantity);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Failed to complete transfer: {ex.Message}", ex);
                }
            });
        }

        public async Task CancelTransferAsync(int transferId)
        {
            var transfer = await _context.StoreTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new InvalidOperationException($"Store transfer with ID {transferId} was not found.");

            var cancelledStatus = await EnsureStatusAsync(
                "CANCELLED",
                "ملغي",
                "Cancelled",
                "Store transfer cancelled");

            transfer.StatusId = cancelledStatus.Id;
            await _context.SaveChangesAsync();
        }

        private async Task CreateInventoryTransaction(int productId, int storeId, decimal quantity, decimal unitCost, string transactionTypeCode, int transferId)
        {
            var transactionType = await EnsureTransactionTypeAsync(
                transactionTypeCode,
                transactionTypeCode == "TRANSFER_IN" ? "تحويل وارد" : "تحويل صادر",
                transactionTypeCode == "TRANSFER_IN" ? "Transfer In" : "Transfer Out",
                transactionTypeCode == "TRANSFER_IN"
                    ? "Stock transferred into this warehouse/location"
                    : "Stock transferred out of this warehouse/location");

            // Get current inventory quantity
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.StoreId == storeId);

            var quantityBefore = inventory?.Quantity ?? 0;
            var quantityAfter = quantityBefore + quantity;

            var inventoryTransaction = new InventoryTransaction
            {
                ProductId = productId,
                StoreId = storeId,
                TransactionTypeId = transactionType.Id,
                QuantityChanged = quantity,
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                UnitCost = unitCost,
                TransactionDate = DateTime.UtcNow,
                TransferId = transferId,
                Notes = $"Transfer transaction for transfer ID: {transferId}"
            };

            _context.InventoryTransactions.Add(inventoryTransaction);
        }

        private async Task UpdateInventory(int storeId, int productId, decimal quantityChange)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.StoreId == storeId);

            if (inventory == null)
            {
                // Create new inventory record
                inventory = new Inventory
                {
                    ProductId = productId,
                    StoreId = storeId,
                    Quantity = quantityChange
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity += quantityChange;
            }
        }

        private async Task<Status> EnsureStatusAsync(string code, string nameAr, string nameEn, string description)
        {
            var status = await _context.Statuses.FirstOrDefaultAsync(s => s.Code == code);
            if (status != null)
            {
                return status;
            }

            status = new Status
            {
                Code = code,
                NameAr = nameAr,
                NameEn = nameEn,
                Description = description
            };

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();
            return status;
        }

        private async Task<TransactionType> EnsureTransactionTypeAsync(string code, string nameAr, string nameEn, string description)
        {
            var transactionType = await _context.TransactionTypes.FirstOrDefaultAsync(t => t.Code == code);
            if (transactionType != null)
            {
                return transactionType;
            }

            transactionType = new TransactionType
            {
                Code = code,
                NameAr = nameAr,
                NameEn = nameEn,
                Description = description
            };

            _context.TransactionTypes.Add(transactionType);
            await _context.SaveChangesAsync();
            return transactionType;
        }
    }
}
