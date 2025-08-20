using System.Linq.Expressions;
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
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CompanyController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var company = await _unitOfWork.CompanyRepo.GetAll().FirstOrDefaultAsync();
                if (company == null)
                    return NotFound("Company settings not found");

                return Ok(company);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCompanyById")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            try
            {
                var company = await _unitOfWork.CompanyRepo.GetByCondition(x => x.Id == id).FirstOrDefaultAsync();
                if (company == null)
                    return NotFound("Company not found");

                return Ok(company);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCompaniesPagination")]
        public async Task<IActionResult> GetCompaniesPagination(int pageIndex, int pageSize)
        {
            try
            {
                var list = await _unitOfWork.CompanyRepo.GetAllPagination(pageIndex, pageSize, x => x.Id, null);
                var total = await _unitOfWork.CompanyRepo.GetTotalCount();
                return Ok(new
                {
                    companies = list,
                    totals = total
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("SearchCompaniesPagination")]
        public async Task<IActionResult> SearchCompaniesPagination(int pageIndex, int pageSize, string keyword)
        {
            try
            {
                Expression<Func<Company, bool>> filter = x =>
                    x.NameEn.Contains(keyword) || x.NameAr.Contains(keyword);

                // 1. Get paginated result
                var list = await _unitOfWork.CompanyRepo.Search(
                    pageIndex,
                    pageSize,
                    filter,
                    x => x.Id // Order by Id
                );

                // 2. Get total count (matching all entries without pagination)
                // var total = await _unitOfWork.CompanyRepo.GetByCondition(filter).CountAsync();

                return Ok(new
                {
                    companies = list,
                    totals = list.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("SaveCompany")]
        public async Task<IActionResult> SaveCompany([FromForm] CompanyDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid input");

                if (dto.Id > 0)
                {
                    // Update
                    var updated = await _unitOfWork.CompanyRepo.UpdateCompany(dto);
                    return Ok(updated);
                }
                else
                {
                    // Add
                    var created = await _unitOfWork.CompanyRepo.AddCompany(dto);
                    return Ok(created);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}