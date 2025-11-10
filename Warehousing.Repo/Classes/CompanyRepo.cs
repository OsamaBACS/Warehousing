using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class CompanyRepo : RepositoryBase<Company>, ICompanyRepo
    {
        public CompanyRepo(WarehousingContext context, ILogger<CompanyRepo> logger, IConfiguration config) : base(context, logger, config) { }

        public async Task<Company> AddCompany(CompanyDto dto)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Company");
                string imageUrl = "";

                Company company = new Company();

                if (dto.Image != null)
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileNameWithPath = "";
                    string fileName = "";
                    var guid = Guid.NewGuid().ToString();
                    fileName = guid + "_" + dto.Image.FileName;
                    imageUrl = Path.Combine(path, fileName);
                    fileNameWithPath = imageUrl;
                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        dto.Image.CopyTo(stream);
                    }

                    company.LogoUrl = Path.Combine("Resources", "Images", "Company", fileName).Replace("\\", "/");
                }

                company.NameEn = dto.NameEn;
                company.NameAr = dto.NameAr;
                company.AddressEn = dto.AddressEn;
                company.AddressAr = dto.AddressAr;
                company.Phone = dto.Phone;
                company.Email = dto.Email;
                company.Website = dto.Website;
                company.TaxNumber = dto.TaxNumber;
                company.Fax = dto.Fax ?? string.Empty;
                company.RegistrationNumber = dto.RegistrationNumber ?? string.Empty;
                company.Capital = dto.Capital;
                company.SloganEn = dto.SloganEn ?? string.Empty;
                company.SloganAr = dto.SloganAr ?? string.Empty;
                company.CurrencyCode = dto.CurrencyCode;
                company.FooterNoteEn = dto.FooterNoteEn;
                company.FooterNoteAr = dto.FooterNoteAr;
                company.TermsEn = dto.TermsEn;
                company.TermsAr = dto.TermsAr;
                company.PrintTemplateId = dto.PrintTemplateId;
                company.IsActive = dto.IsActive;

                var created = await CreateAsync(company);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add company: {CompanyNameEn}", dto?.NameEn);
                throw;
            }
        }

        public async Task<Company> UpdateCompany(CompanyDto dto)
        {
            try
            {
                Company company = GetByCondition(x => x.Id == dto.Id).FirstOrDefault();

                if (company == null)
                {
                    _logger.LogWarning("Company with ID {CompanyId} not found", dto?.Id);
                    return null;
                }

                if (dto.Image != null)
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), company.LogoUrl ?? "");
                    if (File.Exists(oldImagePath))
                        File.Delete(oldImagePath);

                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Company");
                    string imageUrl = "";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileNameWithPath = "";
                    string fileName = "";
                    var guid = Guid.NewGuid().ToString();
                    fileName = guid + "_" + dto.Image.FileName;
                    imageUrl = Path.Combine(path, fileName);
                    fileNameWithPath = imageUrl;
                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        dto.Image.CopyTo(stream);
                    }

                    company.LogoUrl = Path.Combine("Resources", "Images", "Company", fileName).Replace("\\", "/");
                }

                company.NameEn = dto.NameEn;
                company.NameAr = dto.NameAr;
                company.AddressEn = dto.AddressEn;
                company.AddressAr = dto.AddressAr;
                company.Phone = dto.Phone;
                company.Email = dto.Email;
                company.Website = dto.Website;
                company.TaxNumber = dto.TaxNumber;
                company.Fax = dto.Fax ?? string.Empty;
                company.RegistrationNumber = dto.RegistrationNumber ?? string.Empty;
                company.Capital = dto.Capital;
                company.SloganEn = dto.SloganEn ?? string.Empty;
                company.SloganAr = dto.SloganAr ?? string.Empty;
                company.CurrencyCode = dto.CurrencyCode;
                company.FooterNoteEn = dto.FooterNoteEn;
                company.FooterNoteAr = dto.FooterNoteAr;
                company.TermsEn = dto.TermsEn;
                company.TermsAr = dto.TermsAr;
                company.PrintTemplateId = dto.PrintTemplateId;
                company.IsActive = dto.IsActive;

                var updated = await UpdateAsync(company);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update company: {CompanyNameEn}", dto?.NameEn);
                throw;
            }
        }
    }
}