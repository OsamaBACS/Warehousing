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
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var list = await _unitOfWork.CategoryRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetActiveCategories")]
        public async Task<IActionResult> GetActiveCategories()
        {
            try
            {
                var list = await _unitOfWork.CategoryRepo.GetAll().Where(c => c.IsActive).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            try
            {
                var Category = await _unitOfWork.CategoryRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (Category == null)
                {
                    return NotFound("Category Not Found!");
                }
                else
                {
                    return Ok(Category);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveCategory")]
        public async Task<IActionResult> SaveCategory(CategoryDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Category Model is null!");
                }

                var isCategoryExist = await _unitOfWork.CategoryRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isCategoryExist != null)
                    return BadRequest("Category already exists.");

                if (dto.Id > 0)
                {
                    var CategoryToUpdate = _unitOfWork.CategoryRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (CategoryToUpdate != null)
                    {
                        _mapper.Map(dto, CategoryToUpdate);
                        var result = await _unitOfWork.CategoryRepo.UpdateAsync(CategoryToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This Category is not exist!");
                    }
                }
                else
                {
                    var category = new Category
                    {
                        NameEn = dto.NameEn,
                        NameAr = dto.NameAr,
                        Description = dto.Description,
                    };
                    var result = await _unitOfWork.CategoryRepo.CreateAsync(category);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Category");
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