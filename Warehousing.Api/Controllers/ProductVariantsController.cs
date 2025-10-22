using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductVariantsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("by-product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var list = await _unitOfWork.ProductVariantRepo
                .GetByCondition(v => v.ProductId == productId)
                .ProjectTo<ProductVariantSimpleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var entity = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(_mapper.Map<ProductVariantDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductVariantDto dto)
        {
            var entity = _mapper.Map<Warehousing.Data.Entities.ProductVariant>(dto);
            var created = await _unitOfWork.ProductVariantRepo.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductVariantDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductVariantDto dto)
        {
            var entity = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _unitOfWork.ProductVariantRepo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductVariantDto>(entity));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitOfWork.ProductVariantRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return Ok(new { success = true });
        }
    }
}
