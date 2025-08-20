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
    public class StoresController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoresController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetStores")]
        public async Task<IActionResult> GetStores()
        {
            try
            {
                var list = await _unitOfWork.StoreRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStoreById")]
        public async Task<IActionResult> GetStoreById(int Id)
        {
            try
            {
                var store = await _unitOfWork.StoreRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (store == null)
                {
                    return NotFound("Store Not Found!");
                }
                else
                {
                    return Ok(store);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveStore")]
        public async Task<IActionResult> SaveStore(StoreDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Store Model is null!");
                }

                var isStoreExist = await _unitOfWork.StoreRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isStoreExist != null)
                    return BadRequest("Store already exists.");

                if (dto.Id > 0)
                {
                    var StoreToUpdate = _unitOfWork.StoreRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (StoreToUpdate != null)
                    {
                        _mapper.Map(dto, StoreToUpdate);
                        var result = await _unitOfWork.StoreRepo.UpdateAsync(StoreToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This Store is not exist!");
                    }
                }
                else
                {
                    var store = new Store();
                    _mapper.Map(dto, store);
                    var result = await _unitOfWork.StoreRepo.CreateAsync(store);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Store");
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