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
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Warehousing.Repo.Interfaces.IFileStorageService? _fileStorageService;

        public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper, Warehousing.Repo.Interfaces.IFileStorageService? fileStorageService = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        [Route("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var list = await _unitOfWork.CategoryRepo
                    .GetAll()
                    .ProjectTo<CategorySimpleDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
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
                var list = await _unitOfWork.CategoryRepo
                    .GetByCondition(c => c.IsActive)
                    .ProjectTo<CategorySimpleDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
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
                var category = await _unitOfWork.CategoryRepo
                    .GetByCondition(u => u.Id == Id)
                    .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                
                if (category == null)
                {
                    return NotFound("Category Not Found!");
                }
                
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("SaveCategory")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveCategory([FromForm] CategoryDto dto)
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

                // Handle image upload
                if (dto.Image != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                    
                    if (_fileStorageService != null)
                    {
                        // Use FileStorageService (Azure or Local)
                        using (var stream = dto.Image.OpenReadStream())
                        {
                            dto.ImagePath = await _fileStorageService.SaveFileAsync(stream, fileName, "Category");
                        }
                    }
                    else
                    {
                        // Fallback to local storage (legacy code)
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Category");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        string fullPath = Path.Combine(path, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await dto.Image.CopyToAsync(stream);
                        }
                        dto.ImagePath = Path.Combine("Resources", "Images", "Category", fileName);
                    }
                }

                if (dto.Id > 0)
                {
                    var CategoryToUpdate = _unitOfWork.CategoryRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (CategoryToUpdate != null)
                    {
                        // If updating and new image is uploaded, delete old image
                        if (dto.Image != null && !string.IsNullOrEmpty(CategoryToUpdate.ImagePath))
                        {
                            if (_fileStorageService != null)
                            {
                                await _fileStorageService.DeleteFileAsync(CategoryToUpdate.ImagePath);
                            }
                            else
                            {
                                // Fallback to local delete
                                string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), CategoryToUpdate.ImagePath);
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }
                        }
                        
                        _mapper.Map(dto, CategoryToUpdate);
                        var result = await _unitOfWork.CategoryRepo.UpdateAsync(CategoryToUpdate);
                        return Ok(_mapper.Map<CategoryDto>(result));
                    }
                    else
                    {
                        return NotFound("This Category is not exist!");
                    }
                }
                else
                {
                    var category = _mapper.Map<Category>(dto);
                    var result = await _unitOfWork.CategoryRepo.CreateAsync(category);
                    if (result != null)
                    {
                        return Ok(_mapper.Map<CategoryDto>(result));
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                // Check if category has subcategories
                var hasSubCategories = await _unitOfWork.SubCategoryRepo
                    .GetByCondition(s => s.CategoryId == id)
                    .AnyAsync();

                if (hasSubCategories)
                    return BadRequest("Cannot delete category with existing subcategories. Please delete subcategories first.");

                // Check if category has products
                var hasProducts = await _unitOfWork.ProductRepo
                    .GetByCondition(p => p.SubCategory.CategoryId == id)
                    .AnyAsync();

                if (hasProducts)
                    return BadRequest("Cannot delete category with existing products. Please delete products first.");

                await _unitOfWork.CategoryRepo.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("with-subcategories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesWithSubCategories()
        {
            try
            {
                var categories = await _unitOfWork.CategoryRepo
                    .GetByConditionIncluding(c => c.IsActive, c => c.SubCategories)
                    .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> SearchCategories(string searchTerm)
        {
            try
            {
                var categories = await _unitOfWork.CategoryRepo
                    .GetByCondition(c => c.NameAr.Contains(searchTerm) || 
                                         c.NameEn.Contains(searchTerm) ||
                                         c.Description.Contains(searchTerm))
                    .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}