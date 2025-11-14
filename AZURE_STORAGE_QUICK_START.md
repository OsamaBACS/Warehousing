# Azure Blob Storage - Quick Start Guide

## ✅ What's Been Set Up

1. **Azure.Storage.Blobs** package installed
2. **AzureBlobStorageService** - Direct Azure Blob Storage service
3. **FileStorageService** - Unified service (Azure + Local fallback)
4. **Services registered** in `Program.cs`
5. **Configuration structure** ready

## 🚀 Quick Setup (5 minutes)

### 1. Create Azure Storage Account

```bash
# Using Azure CLI (optional)
az storage account create \
  --name warehousingstorage \
  --resource-group your-resource-group \
  --location eastus \
  --sku Standard_LRS
```

Or use Azure Portal (see `AZURE_STORAGE_SETUP.md` for details)

### 2. Get Connection String

From Azure Portal:
- Storage Account → Access Keys → Copy "Connection string"

### 3. Configure appsettings.json

Add to `appsettings.json`:

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "images",
    "BaseUrl": "https://YOUR_ACCOUNT.blob.core.windows.net/images"
  }
}
```

### 4. Create Container

In Azure Portal:
- Storage Account → Containers → + Container
- Name: `images`
- Public access: **Blob** (for public images)

### 5. Test It!

The service automatically:
- ✅ Uses Azure if connection string is configured
- ✅ Falls back to local storage if Azure is unavailable
- ✅ Works with existing code (no breaking changes)

## 📝 Next Steps

1. **Update Repositories** - See `MIGRATE_TO_AZURE_STORAGE.md`
2. **Test Upload** - Upload an image and check Azure Portal
3. **Migrate Existing Files** - Use migration script (optional)
4. **Configure CDN** - For faster global delivery (optional)

## 🔒 Security for Production

**DO NOT** commit connection strings to Git!

### Option 1: Azure App Service Configuration (Recommended)
1. Azure Portal → App Service → Configuration
2. Add Application Settings:
   - `AzureStorage:ConnectionString` = your connection string
   - `AzureStorage:ContainerName` = images
   - `AzureStorage:BaseUrl` = your blob URL

### Option 2: Azure Key Vault (Most Secure)
```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Option 3: User Secrets (Local Development)
```bash
dotnet user-secrets set "AzureStorage:ConnectionString" "your-connection-string"
```

## 💰 Cost Estimate

For typical usage:
- **Storage**: 10GB images = ~$0.18/month
- **Operations**: 100K uploads = ~$0.04/month
- **Data Transfer**: First 5GB free
- **Total**: ~$0.25-0.50/month for small-medium apps

## 🎯 Benefits

- ✅ **Scalable**: Handle millions of files
- ✅ **Reliable**: 99.9% SLA
- ✅ **Fast**: Global CDN support
- ✅ **Secure**: Built-in encryption
- ✅ **Cost-effective**: Pay per use
- ✅ **No breaking changes**: Falls back to local if needed

## 📚 Documentation

- Full setup: `AZURE_STORAGE_SETUP.md`
- Migration guide: `MIGRATE_TO_AZURE_STORAGE.md`
- Azure docs: https://docs.microsoft.com/azure/storage/blobs/

