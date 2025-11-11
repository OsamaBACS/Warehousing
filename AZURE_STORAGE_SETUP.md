# Azure Blob Storage Setup Guide

## Why Azure Blob Storage?

Azure Blob Storage is the best choice for storing images and files in Azure because:

1. **Cost-Effective**: Pay only for what you use, very affordable for images
2. **Scalable**: Handles millions of files without performance issues
3. **Secure**: Built-in security features, access control, and encryption
4. **CDN Integration**: Easy to integrate with Azure CDN for fast global delivery
5. **Reliable**: 99.9% SLA with geo-redundancy options
6. **Easy Integration**: Simple .NET SDK available

## Pricing (Approximate)

- **Hot Tier**: $0.0184 per GB/month (for frequently accessed files)
- **Cool Tier**: $0.01 per GB/month (for infrequently accessed files)
- **Transactions**: $0.004 per 10,000 operations
- **Data Transfer**: First 5GB free, then varies by region

**Example**: 10GB of images + 1M operations/month ≈ **$0.20-0.30/month**

## Step 1: Create Azure Storage Account

1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → Search "Storage account"
3. Fill in:
   - **Subscription**: Your subscription
   - **Resource group**: Create new or use existing
   - **Storage account name**: `warehousingstorage` (must be globally unique)
   - **Region**: Choose closest to your app (e.g., `East US`, `West Europe`)
   - **Performance**: Standard (sufficient for images)
   - **Redundancy**: LRS (Locally Redundant Storage) for cost savings, or GRS for higher availability
4. Click "Review + create" → "Create"
5. Wait for deployment (1-2 minutes)

## Step 2: Create Blob Container

1. Go to your Storage Account
2. Click "Containers" in the left menu
3. Click "+ Container"
4. Name: `images` (or `warehousing-images`)
5. **Public access level**: 
   - **Blob** (recommended) - Allows public read access to blobs
   - **Private** - Only accessible via SAS tokens or managed identity
6. Click "Create"

## Step 3: Get Connection String

1. In your Storage Account, go to "Access keys"
2. Copy "Connection string" from key1 or key2
3. Save it securely (you'll add it to appsettings.json)

## Step 4: Install NuGet Package

```bash
cd Warehousing.Api
dotnet add package Azure.Storage.Blobs
```

## Step 5: Configure appsettings.json

Add to your `appsettings.json`:

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=warehousingstorage;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "images",
    "BaseUrl": "https://warehousingstorage.blob.core.windows.net/images"
  }
}
```

**For Production**: Use Azure App Service Configuration or Key Vault (see Security section)

## Step 6: Implementation

The implementation includes:
- `AzureBlobStorageService` - Service for uploading/downloading files
- Updated repositories to use Azure Blob Storage
- Automatic migration from local to Azure
- Fallback to local storage if Azure is unavailable

## Security Best Practices

1. **Never commit connection strings to Git**
   - Use Azure App Service Configuration
   - Use Azure Key Vault for production
   - Use User Secrets for local development

2. **Access Control**:
   - Use **Private** containers with SAS tokens for sensitive files
   - Use **Blob** public access for public images (product images, logos)
   - Implement Azure AD authentication for admin operations

3. **CORS Configuration**:
   - Configure CORS in Azure Portal → Storage Account → Resource sharing (CORS)
   - Allow only your domain(s)

## Optional: Azure CDN Setup

For faster image delivery globally:

1. Create Azure CDN Profile
2. Add endpoint pointing to your Blob Storage
3. Update `BaseUrl` in appsettings to CDN URL
4. Images will be cached globally for faster access

## Migration Strategy

1. **Phase 1**: Deploy with both local and Azure support (fallback)
2. **Phase 2**: Migrate existing images to Azure (script provided)
3. **Phase 3**: Switch to Azure-only mode
4. **Phase 4**: Remove local storage code (optional)

## Monitoring

- Monitor storage usage in Azure Portal
- Set up alerts for unusual activity
- Track costs in Cost Management

## Backup Strategy

- Enable **Soft Delete** in Blob Storage settings
- Enable **Versioning** for important files
- Consider **Geo-redundant Storage (GRS)** for critical data

