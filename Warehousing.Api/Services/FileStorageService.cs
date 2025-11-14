using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehousing.Api.Services;
using Warehousing.Repo.Interfaces;

namespace Warehousing.Api.Services
{
    /// <summary>
    /// Unified file storage service that supports both Azure Blob Storage and local file storage
    /// Automatically uses Azure if configured, otherwise falls back to local storage
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IAzureBlobStorageService? _azureBlobStorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly bool _useAzureStorage;

        public FileStorageService(
            IConfiguration configuration,
            ILogger<FileStorageService> logger,
            IAzureBlobStorageService? azureBlobStorageService = null)
        {
            _configuration = configuration;
            _logger = logger;
            _azureBlobStorageService = azureBlobStorageService;
            
            // Check if Azure Storage is enabled
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _useAzureStorage = !string.IsNullOrEmpty(connectionString) && _azureBlobStorageService != null;

            if (_useAzureStorage)
            {
                _logger.LogInformation("File storage: Using Azure Blob Storage");
            }
            else
            {
                _logger.LogInformation("File storage: Using local file system");
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
        {
            if (_useAzureStorage && _azureBlobStorageService != null)
            {
                try
                {
                    // Use Azure Blob Storage
                    var blobPath = await _azureBlobStorageService.UploadFileAsync(fileStream, fileName, folder);
                    
                    // Return path in format: azure://folder/filename (for identification)
                    // Or just return the blob path if you want to store it directly
                    return blobPath;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to upload to Azure, falling back to local storage");
                    // Fall through to local storage
                }
            }

            // Use local file storage
            return await SaveFileLocallyAsync(fileStream, fileName, folder);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            // Check if it's an Azure blob path (no "Resources" prefix) or local path
            bool isAzurePath = !filePath.StartsWith("Resources", StringComparison.OrdinalIgnoreCase);

            if (isAzurePath && _useAzureStorage && _azureBlobStorageService != null)
            {
                try
                {
                    return await _azureBlobStorageService.DeleteFileAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete from Azure, trying local storage");
                }
            }

            // Delete from local storage
            return await DeleteFileLocallyAsync(filePath);
        }

        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            // Check if it's an Azure blob path
            bool isAzurePath = !filePath.StartsWith("Resources", StringComparison.OrdinalIgnoreCase);

            if (isAzurePath && _useAzureStorage && _azureBlobStorageService != null)
            {
                try
                {
                    return _azureBlobStorageService.GetFileUrl(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get Azure URL, using local path");
                }
            }

            // Return local file URL (relative path)
            return $"/{filePath.Replace("\\", "/")}";
        }

        public bool IsAzureStorageEnabled()
        {
            return _useAzureStorage;
        }

        private async Task<string> SaveFileLocallyAsync(Stream fileStream, string fileName, string folder)
        {
            try
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", folder);
                
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                var guid = Guid.NewGuid().ToString();
                var uniqueFileName = $"{guid}_{fileName}";
                var fullPath = Path.Combine(basePath, uniqueFileName);

                using (var outputStream = new FileStream(fullPath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(outputStream);
                }

                // Return relative path
                return Path.Combine("Resources", "Images", folder, uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file locally: {FileName}", fileName);
                throw;
            }
        }

        private async Task<bool> DeleteFileLocallyAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted locally: {FilePath}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file locally: {FilePath}", filePath);
                return false;
            }
        }
    }
}

