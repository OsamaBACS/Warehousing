using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class UnitsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UnitsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetUnits")]
        public async Task<IActionResult> GetUnits()
        {
            try
            {
                var list = await _unitOfWork.UnitRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUnitById")]
        public async Task<IActionResult> GetUnitById(int Id)
        {
            try
            {
                var unit = await _unitOfWork.UnitRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (unit == null)
                {
                    return NotFound("Unit Not Found!");
                }
                else
                {
                    return Ok(unit);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveUnit")]
        public async Task<IActionResult> SaveUnit(UnitDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Unit Model is null!");
                }

                var isUnitExist = await _unitOfWork.UnitRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isUnitExist != null)
                    return BadRequest("Unit already exists.");

                if (dto.Id > 0)
                {
                    var UnitToUpdate = _unitOfWork.UnitRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (UnitToUpdate != null)
                    {
                        _mapper.Map(dto, UnitToUpdate);
                        var result = await _unitOfWork.UnitRepo.UpdateAsync(UnitToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This Unit is not exist!");
                    }
                }
                else
                {
                    var unit = new Unit
                    {
                        NameEn = dto.NameEn,
                        NameAr = dto.NameAr,
                        Code = dto.Code,
                        Description = dto.Description,
                    };
                    var result = await _unitOfWork.UnitRepo.CreateAsync(unit);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Unit");
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