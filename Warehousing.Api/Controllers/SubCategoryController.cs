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
    public class SubCategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubCategoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetSubCategories")]
        public async Task<IActionResult> GetSubCategories()
        {
            try
            {
                var list = await _unitOfWork.SubCategoryRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetActiveSubCategories")]
        public async Task<IActionResult> GetActiveSubCategories()
        {
            try
            {
                var list = await _unitOfWork.SubCategoryRepo.GetAll().Where(c => c.IsActive).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSubCategoryById")]
        public async Task<IActionResult> GetSubCategoryById(int Id)
        {
            try
            {
                var Category = await _unitOfWork.SubCategoryRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (Category == null)
                {
                    return NotFound("Sub Category Not Found!");
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

        [HttpGet]
        [Route("GetSubCategoryByCategoryId")]
        public async Task<IActionResult> GetSubCategoryByCategoryId(int CategoryId)
        {
            try
            {
                var subCategories = await _unitOfWork.SubCategoryRepo.GetByCondition(u => u.CategoryId == CategoryId && u.IsActive).ToListAsync();
                if (subCategories == null)
                {
                    return NotFound("No sub Categories Found!");
                }
                else
                {
                    return Ok(subCategories);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveSubCategory")]
        public async Task<IActionResult> SaveSubCategory(SubCategoryDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Sub Category Model is null!");
                }

                var isSubCategoryExist = await _unitOfWork.SubCategoryRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isSubCategoryExist != null)
                    return BadRequest("Sub Category already exists.");

                if (dto.Id > 0)
                {
                    var SubCategoryToUpdate = _unitOfWork.SubCategoryRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (SubCategoryToUpdate != null)
                    {
                        _mapper.Map(dto, SubCategoryToUpdate);
                        var result = await _unitOfWork.SubCategoryRepo.UpdateAsync(SubCategoryToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This Sub Category is not exist!");
                    }
                }
                else
                {
                    var subCategory = new SubCategory
                    {
                        NameEn = dto.NameEn,
                        NameAr = dto.NameAr,
                        Description = dto.Description,
                        CategoryId = dto.CategoryId
                    };
                    var result = await _unitOfWork.SubCategoryRepo.CreateAsync(subCategory);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Sub Category");
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