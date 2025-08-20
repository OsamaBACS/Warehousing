using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetSuppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                var list = await _unitOfWork.SupplierRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSupplierById")]
        public async Task<IActionResult> GetSupplierById(int Id)
        {
            try
            {
                var Supplier = await _unitOfWork.SupplierRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (Supplier == null)
                {
                    return NotFound("Supplier Not Found!");
                }
                else
                {
                    return Ok(Supplier);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveSupplier")]
        public async Task<IActionResult> SaveSupplier(SupplierDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Supplier Model is null!");
                }

                var isSupplierExist = await _unitOfWork.SupplierRepo
                    .GetByCondition(r => (r.Name == dto.Name) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isSupplierExist != null)
                    return BadRequest("Supplier already exists.");

                if (dto.Id > 0)
                {
                    var SupplierToUpdate = _unitOfWork.SupplierRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (SupplierToUpdate != null)
                    {
                        _mapper.Map(dto, SupplierToUpdate);
                        var result = await _unitOfWork.SupplierRepo.UpdateAsync(SupplierToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This Supplier is not exist!");
                    }
                }
                else
                {
                    var Supplier = new Data.Entities.Supplier
                    {
                        Name = dto.Name,
                        Address = dto.Address,
                        Email = dto.Email,
                        Phone = dto.Phone
                    };
                    var result = await _unitOfWork.SupplierRepo.CreateAsync(Supplier);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Supplier");
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