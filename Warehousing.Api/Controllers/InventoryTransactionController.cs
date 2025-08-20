using System.Linq.Expressions;
using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryTransactionController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetTransactionByProductId")]
        public async Task<IActionResult> GetTransactionByProductId(int pageIndex, int pageSize, int id, int storeId = 1)
        {
            try
            {
                Expression<Func<InventoryTransaction, bool>> filter = x =>
                    x.ProductId == id &&
                    (
                        storeId == 0 || // 0 means "All"
                        (
                            x.Order != null &&
                            x.Order.Items.Any(i => i.StoreId == storeId)
                        )
                    );

                var list = await _unitOfWork.InventoryTransactionRepo
                    .GetAllPagination(
                        pageIndex,
                        pageSize,
                        x => x.Id,
                        filter,
                        [p => p.Product, tt => tt.TransactionType, po => po.Order, po => po.Order!.Items]
                    );

                var TotalSize = await _unitOfWork.ProductRepo.GetTotalCount();

                return Ok(new
                {
                    transactions = list,
                    totals = TotalSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}