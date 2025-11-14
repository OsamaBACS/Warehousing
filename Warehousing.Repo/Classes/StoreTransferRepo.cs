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

        public async Task<bool> CompleteTransferAsync(int transferId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.StoreTransfers
                    .Include(t => t.Items)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null)
                    return false;

                // Update status to completed
                var completedStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.Code == "COMPLETED");

                if (completedStatus == null)
                    return false;

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
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> CancelTransferAsync(int transferId)
        {
            var transfer = await _context.StoreTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                return false;

            var cancelledStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.Code == "CANCELLED");

            if (cancelledStatus == null)
                return false;

            transfer.StatusId = cancelledStatus.Id;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task CreateInventoryTransaction(int productId, int storeId, decimal quantity, decimal unitCost, string transactionTypeCode, int transferId)
        {
            var transactionType = await _context.TransactionTypes
                .FirstOrDefaultAsync(tt => tt.Code == transactionTypeCode);

            if (transactionType == null)
                return;

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
    }
}
