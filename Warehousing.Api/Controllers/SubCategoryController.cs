using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
                var list = await _unitOfWork.SubCategoryRepo
                    .GetAll()
                    .Include(s => s.Category) // Only include Category for CategoryName mapping
                    .ProjectTo<SubCategorySimpleDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
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
                var list = await _unitOfWork.SubCategoryRepo
                    .GetByCondition(c => c.IsActive)
                    .Include(s => s.Category) // Only include Category for CategoryName mapping
                    .ProjectTo<SubCategorySimpleDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
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
                var subCategory = await _unitOfWork.SubCategoryRepo
                    .GetByCondition(u => u.Id == Id)
                    .ProjectTo<SubCategoryDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                
                if (subCategory == null)
                {
                    return NotFound("Sub Category Not Found!");
                }
                
                return Ok(subCategory);
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
                var subCategories = await _unitOfWork.SubCategoryRepo
                    .GetByCondition(u => u.CategoryId == CategoryId && u.IsActive)
                    .Include(s => s.Category) // Only include Category for CategoryName mapping
                    .ProjectTo<SubCategorySimpleDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                
                if (subCategories == null || !subCategories.Any())
                {
                    return NotFound("No sub Categories Found!");
                }
                
                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("SaveSubCategory")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveSubCategory([FromForm] SubCategoryDto dto)
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

                // Handle image upload
                if (dto.Image != null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "SubCategory");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                    string fullPath = Path.Combine(path, fileName);
                    
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    
                    dto.ImagePath = Path.Combine("Resources", "Images", "SubCategory", fileName);
                }

                if (dto.Id > 0)
                {
                    var SubCategoryToUpdate = _unitOfWork.SubCategoryRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (SubCategoryToUpdate != null)
                    {
                        // If updating and new image is uploaded, delete old image
                        if (dto.Image != null && !string.IsNullOrEmpty(SubCategoryToUpdate.ImagePath))
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), SubCategoryToUpdate.ImagePath);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        
                        _mapper.Map(dto, SubCategoryToUpdate);
                        var result = await _unitOfWork.SubCategoryRepo.UpdateAsync(SubCategoryToUpdate);
                        return Ok(_mapper.Map<SubCategoryDto>(result));
                    }
                    else
                    {
                        return NotFound("This Sub Category is not exist!");
                    }
                }
                else
                {
                    var subCategory = _mapper.Map<SubCategory>(dto);
                    var result = await _unitOfWork.SubCategoryRepo.CreateAsync(subCategory);
                    if (result != null)
                    {
                        return Ok(_mapper.Map<SubCategoryDto>(result));
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSubCategory(int id)
        {
            try
            {
                // Check if subcategory has products
                var hasProducts = await _unitOfWork.ProductRepo
                    .GetByCondition(p => p.SubCategoryId == id)
                    .AnyAsync();

                if (hasProducts)
                    return BadRequest("Cannot delete subcategory with existing products. Please delete products first.");

                await _unitOfWork.SubCategoryRepo.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("with-products")]
        public async Task<ActionResult<IEnumerable<SubCategoryDto>>> GetSubCategoriesWithProducts()
        {
            try
            {
                var subCategories = await _unitOfWork.SubCategoryRepo
                    .GetByConditionIncluding(s => s.IsActive, s => s.Products)
                    .ProjectTo<SubCategoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<SubCategoryDto>>> SearchSubCategories(string searchTerm)
        {
            try
            {
                var subCategories = await _unitOfWork.SubCategoryRepo
                    .GetByCondition(s => s.NameAr.Contains(searchTerm) || 
                                         s.NameEn.Contains(searchTerm) ||
                                         s.Description.Contains(searchTerm))
                    .ProjectTo<SubCategoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-category/{categoryId}/with-products")]
        public async Task<ActionResult<IEnumerable<SubCategoryDto>>> GetSubCategoriesByCategoryWithProducts(int categoryId)
        {
            try
            {
                var subCategories = await _unitOfWork.SubCategoryRepo
                    .GetByConditionIncluding(s => s.CategoryId == categoryId && s.IsActive, s => s.Products)
                    .ProjectTo<SubCategoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}