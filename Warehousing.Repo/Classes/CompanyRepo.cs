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
        private readonly IFileStorageService? _fileStorageService;

        public CompanyRepo(WarehousingContext context, ILogger<CompanyRepo> logger, IConfiguration config, IFileStorageService? fileStorageService = null) 
            : base(context, logger, config) 
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<Company> AddCompany(CompanyDto dto)
        {
            try
            {
                Company company = new Company();

                if (dto.Image != null)
                {
                    var guid = Guid.NewGuid().ToString();
                    var fileName = $"{guid}_{dto.Image.FileName}";
                    
                    if (_fileStorageService != null)
                    {
                        // Use FileStorageService (Azure or Local)
                        using (var stream = dto.Image.OpenReadStream())
                        {
                            company.LogoUrl = await _fileStorageService.SaveFileAsync(stream, fileName, "Company");
                        }
                    }
                    else
                    {
                        // Fallback to local storage (legacy code)
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Company");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        string fileNameWithPath = Path.Combine(path, fileName);
                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            await dto.Image.CopyToAsync(stream);
                        }
                        company.LogoUrl = Path.Combine("Resources", "Images", "Company", fileName).Replace("\\", "/");
                    }
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
                    // Delete old image
                    if (!string.IsNullOrEmpty(company.LogoUrl))
                    {
                        if (_fileStorageService != null)
                        {
                            await _fileStorageService.DeleteFileAsync(company.LogoUrl);
                        }
                        else
                        {
                            // Fallback to local delete
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), company.LogoUrl);
                            if (File.Exists(oldImagePath))
                                File.Delete(oldImagePath);
                        }
                    }

                    // Upload new image
                    var guid = Guid.NewGuid().ToString();
                    var fileName = $"{guid}_{dto.Image.FileName}";
                    
                    if (_fileStorageService != null)
                    {
                        // Use FileStorageService (Azure or Local)
                        using (var stream = dto.Image.OpenReadStream())
                        {
                            company.LogoUrl = await _fileStorageService.SaveFileAsync(stream, fileName, "Company");
                        }
                    }
                    else
                    {
                        // Fallback to local storage (legacy code)
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Company");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        string fileNameWithPath = Path.Combine(path, fileName);
                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            await dto.Image.CopyToAsync(stream);
                        }
                        company.LogoUrl = Path.Combine("Resources", "Images", "Company", fileName).Replace("\\", "/");
                    }
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