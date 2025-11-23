using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreTransferController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStoreTransferRepo _storeTransferRepo;
        private readonly IMapper _mapper;

        public StoreTransferController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _storeTransferRepo = unitOfWork.StoreTransferRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreTransferDto>>> GetAllTransfers()
        {
            var transfers = await _storeTransferRepo.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<StoreTransferDto>>(transfers));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoreTransferDto>> GetTransfer(int id)
        {
            try
            {
                var transfer = await _storeTransferRepo.GetByIdAsync(id);
                if (transfer == null)
                    return NotFound();

                return Ok(_mapper.Map<StoreTransferDto>(transfer));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("with-items/{id}")]
        public async Task<ActionResult<StoreTransferDto>> GetTransferWithItems(int id)
        {
            try
            {
                var transfer = await _storeTransferRepo.GetTransferWithItemsAsync(id);
                return Ok(transfer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-status/{statusId}")]
        public async Task<ActionResult<IEnumerable<StoreTransferDto>>> GetTransfersByStatus(int statusId)
        {
            var transfers = await _storeTransferRepo.GetTransfersByStatusAsync(statusId);
            return Ok(transfers);
        }

        [HttpGet("by-store/{storeId}")]
        public async Task<ActionResult<IEnumerable<StoreTransferDto>>> GetTransfersByStore(int storeId, [FromQuery] bool isFromStore = true)
        {
            var transfers = await _storeTransferRepo.GetTransfersByStoreAsync(storeId, isFromStore);
            return Ok(transfers);
        }

        [HttpPost]
        public async Task<ActionResult<StoreTransferDto>> CreateTransfer([FromBody] StoreTransferDto transferDto)
        {
            try
            {
                // Validate required fields
                if (transferDto.FromStoreId <= 0)
                    return BadRequest("FromStoreId is required and must be greater than 0");
                
                if (transferDto.ToStoreId <= 0)
                    return BadRequest("ToStoreId is required and must be greater than 0");
                
                if (transferDto.FromStoreId == transferDto.ToStoreId)
                    return BadRequest("FromStoreId and ToStoreId cannot be the same");
                
                if (transferDto.Items == null || transferDto.Items.Count == 0)
                    return BadRequest("Transfer must have at least one item");

                // Set default status to Pending if not provided
                if (transferDto.StatusId == 0)
                {
                    var draftStatus = await _unitOfWork.StatusRepo.GetByCodeAsync("PENDING");
                    if (draftStatus != null)
                        transferDto.StatusId = draftStatus.Id;
                    else
                        return BadRequest("Status 'PENDING' not found in database");
                }

                // Map DTO to entity - ensure Items are properly mapped
                var entity = _mapper.Map<StoreTransfer>(transferDto);
                
                // Ensure items have correct TransferId (will be set after creation)
                foreach (var item in entity.Items)
                {
                    item.TransferId = 0; // Will be set after entity is saved
                }

                var createdTransfer = await _storeTransferRepo.CreateAsync(entity);
                
                // Update TransferId for all items
                foreach (var item in createdTransfer.Items)
                {
                    item.TransferId = createdTransfer.Id;
                }
                
                await _unitOfWork.SaveAsync();

                return CreatedAtAction(nameof(GetTransfer), new { id = createdTransfer.Id }, _mapper.Map<StoreTransferDto>(createdTransfer));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StoreTransferDto>> UpdateTransfer(int id, [FromBody] StoreTransferDto transferDto)
        {
            try
            {
                if (id != transferDto.Id)
                    return BadRequest("ID mismatch");

                var entity = _mapper.Map<StoreTransfer>(transferDto);
                var updatedTransfer = await _storeTransferRepo.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();

                return Ok(_mapper.Map<StoreTransferDto>(updatedTransfer));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTransfer(int id)
        {
            try
            {
                await _storeTransferRepo.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteTransfer(int id)
        {
            try
            {
                // CompleteTransferAsync already saves changes inside the transaction
                await _storeTransferRepo.CompleteTransferAsync(id);
                return Ok(new { message = "Transfer completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelTransfer(int id)
        {
            try
            {
                await _storeTransferRepo.CancelTransferAsync(id);
                await _unitOfWork.SaveAsync();
                return Ok(new { message = "Transfer cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
