using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Interfaces
{
    public interface ICompanyRepo: IRepositoryBase<Company>
    {
        Task<Company> AddCompany(CompanyDto companyDto);
        Task<Company> UpdateCompany(CompanyDto companyDto);
    }
}