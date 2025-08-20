using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderItemController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetOrderItemsById")]
        public async Task<IActionResult> GetOrderItemsById(int Id)
        {
            try
            {
                var items = await _unitOfWork.OrderItemRepo.GetAll().Include(p => p.Product).FirstOrDefaultAsync(i => i.Id == Id);
                if (items != null)
                {
                    return Ok(items);
                }
                else
                {
                    return NotFound("No Items found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}