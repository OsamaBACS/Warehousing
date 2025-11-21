using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using Warehousing.Data.Entities;

namespace Warehousing.Repo.Interfaces
{
    public interface IStoreTransferRepo : IRepositoryBase<StoreTransfer>
    {
        Task<IEnumerable<StoreTransferDto>> GetTransfersByStatusAsync(int statusId);
        Task<IEnumerable<StoreTransferDto>> GetTransfersByStoreAsync(int storeId, bool isFromStore = true);
        Task<StoreTransferDto> GetTransferWithItemsAsync(int transferId);
        Task CompleteTransferAsync(int transferId);
        Task CancelTransferAsync(int transferId);
    }
}
