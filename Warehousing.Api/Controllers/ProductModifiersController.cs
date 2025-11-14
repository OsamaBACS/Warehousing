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
    public class ProductModifiersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductModifiersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Modifiers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _unitOfWork.ProductModifierRepo
                .GetAll()
                .ProjectTo<ProductModifierSimpleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var entity = await _unitOfWork.ProductModifierRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(_mapper.Map<ProductModifierDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductModifierDto dto)
        {
            var entity = _mapper.Map<Warehousing.Data.Entities.ProductModifier>(dto);
            var created = await _unitOfWork.ProductModifierRepo.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductModifierDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductModifierDto dto)
        {
            var entity = await _unitOfWork.ProductModifierRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _unitOfWork.ProductModifierRepo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductModifierDto>(entity));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitOfWork.ProductModifierRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return Ok(new { success = true });
        }

        // Options
        [HttpGet("{modifierId}/options")]
        public async Task<IActionResult> GetOptions(int modifierId)
        {
            var list = await _unitOfWork.ProductModifierOptionRepo
                .GetByCondition(o => o.ModifierId == modifierId)
                .ProjectTo<ProductModifierOptionSimpleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost("{modifierId}/options")]
        public async Task<IActionResult> CreateOption(int modifierId, [FromBody] ProductModifierOptionDto dto)
        {
            dto.ModifierId = modifierId;
            var entity = _mapper.Map<Warehousing.Data.Entities.ProductModifierOption>(dto);
            var created = await _unitOfWork.ProductModifierOptionRepo.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductModifierOptionDto>(created));
        }

        [HttpPut("options/{id}")]
        public async Task<IActionResult> UpdateOption(int id, [FromBody] ProductModifierOptionDto dto)
        {
            var entity = await _unitOfWork.ProductModifierOptionRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _unitOfWork.ProductModifierOptionRepo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductModifierOptionDto>(entity));
        }

        [HttpDelete("options/{id}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            await _unitOfWork.ProductModifierOptionRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return Ok(new { success = true });
        }

        // Groups (link modifiers to products)
        [HttpGet("groups/by-product/{productId}")]
        public async Task<IActionResult> GetGroupsByProduct(int productId)
        {
            var list = await _unitOfWork.ProductModifierGroupRepo
                .GetByCondition(g => g.ProductId == productId)
                .Include(g => g.Modifier)
                .ProjectTo<ProductModifierGroupSimpleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] ProductModifierGroupDto dto)
        {
            var entity = _mapper.Map<Warehousing.Data.Entities.ProductModifierGroup>(dto);
            var created = await _unitOfWork.ProductModifierGroupRepo.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductModifierGroupDto>(created));
        }

        [HttpPut("groups/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] ProductModifierGroupDto dto)
        {
            var entity = await _unitOfWork.ProductModifierGroupRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _unitOfWork.ProductModifierGroupRepo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductModifierGroupDto>(entity));
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            await _unitOfWork.ProductModifierGroupRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return Ok(new { success = true });
        }
    }
}
