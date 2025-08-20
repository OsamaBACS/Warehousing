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
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var list = await _unitOfWork.CustomerRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCustomerById")]
        public async Task<IActionResult> GetCustomerById(int Id)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (customer == null)
                {
                    return NotFound("Customer Not Found!");
                }
                else
                {
                    return Ok(customer);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveCustomer")]
        public async Task<IActionResult> SaveCustomer(CustomerDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Customer Model is null!");
                }

                var isCustomerExist = await _unitOfWork.CustomerRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isCustomerExist != null)
                    return BadRequest("Customer already exists.");

                if (dto.Id > 0)
                {
                    var customerToUpdate = _unitOfWork.CustomerRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (customerToUpdate != null)
                    {
                        _mapper.Map(dto, customerToUpdate);
                        var result = await _unitOfWork.CustomerRepo.UpdateAsync(customerToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This customer is not exist!");
                    }
                }
                else
                {
                    var customer = new Customer
                    {
                        NameEn = dto.NameEn,
                        NameAr = dto.NameAr,
                        Address = dto.Address,
                        Email = dto.Email,
                        Phone = dto.Phone
                    };
                    var result = await _unitOfWork.CustomerRepo.CreateAsync(customer);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding customer");
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