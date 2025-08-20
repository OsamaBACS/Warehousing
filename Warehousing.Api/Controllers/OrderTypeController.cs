using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderTypeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetOrderTypes")]
        public async Task<IActionResult> GetOrderTypes()
        {
            try
            {
                var list = await _unitOfWork.OrderTypeRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetActiveOrderType")]
        public async Task<IActionResult> GetActiveOrderType()
        {
            try
            {
                var list = await _unitOfWork.OrderTypeRepo.GetAll().Where(c => c.IsActive).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetOrderTypeById")]
        public async Task<IActionResult> GetOrderTypeById(int Id)
        {
            try
            {
                var OrderType = await _unitOfWork.OrderTypeRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (OrderType == null)
                {
                    return NotFound("OrderType Not Found!");
                }
                else
                {
                    return Ok(OrderType);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveOrderType")]
        public async Task<IActionResult> SaveOrderType(OrderTypeDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("OrderType Model is null!");
                }

                var isOrderTypeExist = await _unitOfWork.OrderTypeRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isOrderTypeExist != null)
                    return BadRequest("OrderType already exists.");

                if (dto.Id > 0)
                {
                    var OrderTypeToUpdate = _unitOfWork.OrderTypeRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (OrderTypeToUpdate != null)
                    {
                        _mapper.Map(dto, OrderTypeToUpdate);
                        var result = await _unitOfWork.OrderTypeRepo.UpdateAsync(OrderTypeToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This OrderType is not exist!");
                    }
                }
                else
                {
                    var orderType = new OrderType
                    {
                        NameEn = dto.NameEn,
                        NameAr = dto.NameAr,
                        Description = dto.Description,
                    };
                    var result = await _unitOfWork.OrderTypeRepo.CreateAsync(orderType);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding OrderType");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }   
    }
}