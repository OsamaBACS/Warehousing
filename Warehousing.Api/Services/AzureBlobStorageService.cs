using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Api.Services
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        string GetFileUrl(string filePath);
    }

    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient? _blobServiceClient;
        private readonly string _containerName;
        private readonly string _baseUrl;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly bool _isEnabled;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;
            
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _containerName = configuration["AzureStorage:ContainerName"] ?? "images";
            _baseUrl = configuration["AzureStorage:BaseUrl"] ?? string.Empty;

            // Check if Azure Storage is configured
            _isEnabled = !string.IsNullOrEmpty(connectionString);

            if (_isEnabled)
            {
                try
                {
                    _blobServiceClient = new BlobServiceClient(connectionString);
                    _logger.LogInformation("Azure Blob Storage service initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Azure Blob Storage. Falling back to local storage.");
                    _isEnabled = false;
                }
            }
            else
            {
                _logger.LogWarning("Azure Storage connection string not configured. Using local storage.");
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
        {
            if (!_isEnabled || _blobServiceClient == null)
            {
                throw new InvalidOperationException("Azure Blob Storage is not configured or enabled.");
            }

            try
            {
                // Get container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Create container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Build blob path: folder/filename
                var blobName = string.IsNullOrEmpty(folder) 
                    ? fileName 
                    : $"{folder.Trim('/')}/{fileName}";

                // Get blob client
                var blobClient = containerClient.GetBlobClient(blobName);

                // Set content type based on file extension
                var contentType = GetContentType(fileName);
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                // Upload file
                await blobClient.UploadAsync(fileStream, uploadOptions);

                _logger.LogInformation("File uploaded to Azure Blob Storage: {BlobName}", blobName);

                // Return the path that will be stored in database
                return blobName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (!_isEnabled || _blobServiceClient == null || string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(filePath);

                var result = await blobClient.DeleteIfExistsAsync();
                
                if (result.Value)
                {
                    _logger.LogInformation("File deleted from Azure Blob Storage: {FilePath}", filePath);
                }

                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            if (!_isEnabled || _blobServiceClient == null || string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(filePath);
                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence in Azure Blob Storage: {FilePath}", filePath);
                return false;
            }
        }

        public string GetFileUrl(string filePath)
        {
            if (!_isEnabled || _blobServiceClient == null || string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            // If baseUrl is configured, use it; otherwise construct from connection string
            if (!string.IsNullOrEmpty(_baseUrl))
            {
                return $"{_baseUrl.TrimEnd('/')}/{filePath}";
            }

            // Fallback: construct URL from container name (requires public access)
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            return blobClient.Uri.ToString();
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}

