# Migration Guide: Updating Repositories to Use Azure Blob Storage

## Overview

This guide shows how to update your repositories to use the new `FileStorageService` which supports both Azure Blob Storage and local file storage.

## Step 1: Update Repository Interface (if needed)

The repository interface doesn't need changes, but you'll inject `IFileStorageService` into the repository.

## Step 2: Update Repository Constructor

Add `IFileStorageService` to the constructor:

```csharp
private readonly IFileStorageService _fileStorageService;

public ProductRepo(
    WarehousingContext context, 
    ILogger<ProductRepo> logger, 
    IConfiguration config,
    IFileStorageService fileStorageService) 
    : base(context, logger, config)
{
    _fileStorageService = fileStorageService;
}
```

## Step 3: Update AddProduct Method

**Before:**
```csharp
if (dto.Image != null)
{
    string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Product");
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
    string fileName = guid + "_" + dto.Image.FileName;
    string fullPath = Path.Combine(path, fileName);
    using (var stream = new FileStream(fullPath, FileMode.Create))
    {
        dto.Image.CopyTo(stream);
    }
    product.ImagePath = Path.Combine("Resources", "Images", "Product", fileName).Replace("\\", "/");
}
```

**After:**
```csharp
if (dto.Image != null)
{
    var guid = Guid.NewGuid().ToString();
    var fileName = $"{guid}_{dto.Image.FileName}";
    
    using (var stream = dto.Image.OpenReadStream())
    {
        product.ImagePath = await _fileStorageService.SaveFileAsync(stream, fileName, "Product");
    }
}
```

## Step 4: Update UpdateProduct Method

**Before:**
```csharp
if (dto.Image != null)
{
    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), product.ImagePath ?? "");
    if (File.Exists(imagePath))
    {
        File.Delete(imagePath);
    }
    // ... upload new image
}
```

**After:**
```csharp
if (dto.Image != null)
{
    // Delete old image
    if (!string.IsNullOrEmpty(product.ImagePath))
    {
        await _fileStorageService.DeleteFileAsync(product.ImagePath);
    }
    
    // Upload new image
    var guid = Guid.NewGuid().ToString();
    var fileName = $"{guid}_{dto.Image.FileName}";
    
    using (var stream = dto.Image.OpenReadStream())
    {
        product.ImagePath = await _fileStorageService.SaveFileAsync(stream, fileName, "Product");
    }
}
```

## Step 5: Register Service in Repository Project (if needed)

If repositories are in a separate project, you'll need to either:
1. Move `IFileStorageService` to a shared project, OR
2. Pass it from the API layer

**Option A: Shared Interface (Recommended)**

Create `Warehousing.Shared` project and move interfaces there.

**Option B: Pass from API**

Inject in controller and pass to repository method.

## Step 6: Update UnitOfWork Registration

Make sure `IFileStorageService` is registered before repositories:

```csharp
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
```

## Example: Complete Updated ProductRepo.AddProduct

```csharp
public async Task<Product> AddProduct(ProductDto dto)
{
    try
    {
        Product product = new Product();
        
        if (dto.Image != null)
        {
            var guid = Guid.NewGuid().ToString();
            var fileName = $"{guid}_{dto.Image.FileName}";
            
            using (var stream = dto.Image.OpenReadStream())
            {
                product.ImagePath = await _fileStorageService.SaveFileAsync(stream, fileName, "Product");
            }
        }

        var lastProductId = GetAll().Select(p => p.Id).Max();
        product.Code = (lastProductId + 1).ToString();
        product.NameEn = dto.NameEn != null ? dto.NameEn : dto.NameAr;
        product.NameAr = dto.NameAr;
        product.Description = dto.Description!;
        product.SubCategoryId = dto.SubCategoryId;
        product.UnitId = dto.UnitId;
        product.CostPrice = dto.CostPrice;
        product.SellingPrice = dto.SellingPrice;
        product.ReorderLevel = dto.ReorderLevel;

        var createdProduct = await CreateAsync(product);
        return createdProduct;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to add product: {ProductCode}", dto?.Code);
        throw;
    }
}
```

## Repositories to Update

1. ✅ `ProductRepo` - Products
2. ✅ `CompanyRepo` - Company logos
3. ✅ `CategoryController` - Category images (in controller)
4. ✅ `SubCategoryController` - SubCategory images (in controller)

## Testing

1. **Local Storage Test**: Remove Azure connection string, verify files save locally
2. **Azure Storage Test**: Add connection string, verify files upload to Azure
3. **Fallback Test**: Add invalid connection string, verify fallback to local
4. **Migration Test**: Upload new file, verify old local files still work

## Migration Script (Optional)

Create a script to migrate existing local files to Azure:

```csharp
// Migration script to move existing files to Azure
public async Task MigrateLocalFilesToAzure()
{
    var products = await _context.Products
        .Where(p => p.ImagePath != null && p.ImagePath.StartsWith("Resources"))
        .ToListAsync();

    foreach (var product in products)
    {
        var localPath = Path.Combine(Directory.GetCurrentDirectory(), product.ImagePath);
        if (File.Exists(localPath))
        {
            using (var stream = File.OpenRead(localPath))
            {
                var fileName = Path.GetFileName(product.ImagePath);
                var newPath = await _fileStorageService.SaveFileAsync(stream, fileName, "Product");
                product.ImagePath = newPath;
                
                // Optionally delete local file after migration
                // File.Delete(localPath);
            }
        }
    }
    
    await _context.SaveChangesAsync();
}
```

