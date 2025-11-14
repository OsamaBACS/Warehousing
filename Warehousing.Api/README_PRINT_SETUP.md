# 🖨️ Print Service Setup Guide for .NET API

## Prerequisites

1. **.NET 9.0** or later
2. **Azure App Service** or **Azure Container Instances**
3. **Chrome/Chromium** for PuppeteerSharp

## Installation Steps

### Step 1: Install Required Packages

```bash
cd Warehousing.Api
dotnet add package PuppeteerSharp
dotnet add package Microsoft.AspNetCore.StaticFiles
```

### Step 2: Update Program.cs

Add the following to your `Program.cs`:

```csharp
// Add services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://your-angular-app.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add CORS
app.UseCors("AllowAngularApp");
```

### Step 3: Add Print Controller

The `PrintController.cs` file has been created with the following features:
- ✅ PDF generation from HTML
- ✅ Order-specific PDF generation
- ✅ Report PDF generation
- ✅ Azure-optimized settings

### Step 4: Configure for Azure Deployment

#### Option A: Azure App Service

1. **Add Application Settings:**
```json
{
  "WEBSITE_RUN_FROM_PACKAGE": "1",
  "WEBSITE_NODE_DEFAULT_VERSION": "18.0.0",
  "WEBSITE_LOAD_CERTIFICATES": "*"
}
```

2. **Update web.config (if using IIS):**
```xml
<system.webServer>
  <handlers>
    <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
  </handlers>
  <aspNetCore processPath="dotnet" arguments=".\Warehousing.Api.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
    <environmentVariables>
      <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
    </environmentVariables>
  </aspNetCore>
</system.webServer>
```

#### Option B: Azure Container Instances (Recommended)

1. **Create Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Warehousing.Api/Warehousing.Api.csproj", "Warehousing.Api/"]
COPY ["Warehousing.Data/Warehousing.Data.csproj", "Warehousing.Data/"]
COPY ["Warehousing.Repo/Warehousing.Repo.csproj", "Warehousing.Repo/"]
RUN dotnet restore "Warehousing.Api/Warehousing.Api.csproj"
COPY . .
WORKDIR "/src/Warehousing.Api"
RUN dotnet build "Warehousing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Warehousing.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install Chrome for PuppeteerSharp
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    ca-certificates \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "Warehousing.Api.dll"]
```

2. **Deploy to Azure Container Instances:**
```bash
# Build and push to Azure Container Registry
az acr build --registry your-registry --image warehousing-api:latest .

# Deploy to Container Instances
az container create \
  --resource-group your-rg \
  --name warehousing-api \
  --image your-registry.azurecr.io/warehousing-api:latest \
  --cpu 2 \
  --memory 4 \
  --ports 80 443 \
  --environment-variables ASPNETCORE_ENVIRONMENT=Production
```

### Step 5: Test the API

1. **Start the API:**
```bash
dotnet run --project Warehousing.Api
```

2. **Test PDF generation:**
```bash
curl -X POST "https://localhost:7000/api/print/generate-pdf" \
  -H "Content-Type: application/json" \
  -d '{
    "htmlContent": "<html><body><h1>Test PDF</h1></body></html>",
    "title": "Test Document",
    "type": "document"
  }' \
  --output test.pdf
```

## Performance Optimization

### 1. Browser Pool Management

Add to `Program.cs`:

```csharp
builder.Services.AddSingleton<IBrowserPool, BrowserPool>();

public class BrowserPool : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Queue<IBrowser> _browsers;
    private readonly int _maxBrowsers = 3;

    public BrowserPool()
    {
        _semaphore = new SemaphoreSlim(_maxBrowsers, _maxBrowsers);
        _browsers = new Queue<IBrowser>();
    }

    public async Task<IBrowser> GetBrowserAsync()
    {
        await _semaphore.WaitAsync();
        
        if (_browsers.Count > 0)
        {
            return _browsers.Dequeue();
        }

        return await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });
    }

    public void ReturnBrowser(IBrowser browser)
    {
        if (browser != null && !browser.IsClosed)
        {
            _browsers.Enqueue(browser);
        }
        _semaphore.Release();
    }

    public void Dispose()
    {
        while (_browsers.Count > 0)
        {
            _browsers.Dequeue()?.Dispose();
        }
    }
}
```

### 2. Caching

Add PDF caching:

```csharp
builder.Services.AddMemoryCache();

// In PrintController
private readonly IMemoryCache _cache;

public async Task<IActionResult> GeneratePdf([FromBody] PrintRequest request)
{
    var cacheKey = $"pdf_{request.Title}_{request.HtmlContent.GetHashCode()}";
    
    if (_cache.TryGetValue(cacheKey, out byte[] cachedPdf))
    {
        return File(cachedPdf, "application/pdf", $"{request.Title}.pdf");
    }

    var pdfBytes = await GeneratePdfFromHtml(request.HtmlContent, request.Title, request.Type);
    
    _cache.Set(cacheKey, pdfBytes, TimeSpan.FromHours(1));
    
    return File(pdfBytes, "application/pdf", $"{request.Title}.pdf");
}
```

## Monitoring and Logging

### 1. Add Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();

// In PrintController
private readonly TelemetryClient _telemetryClient;

public async Task<IActionResult> GeneratePdf([FromBody] PrintRequest request)
{
    using var operation = _telemetryClient.StartOperation<DependencyTelemetry>("PDF Generation");
    
    try
    {
        var pdfBytes = await GeneratePdfFromHtml(request.HtmlContent, request.Title, request.Type);
        operation.Telemetry.Success = true;
        return File(pdfBytes, "application/pdf", $"{request.Title}.pdf");
    }
    catch (Exception ex)
    {
        operation.Telemetry.Success = false;
        _telemetryClient.TrackException(ex);
        throw;
    }
}
```

### 2. Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("pdf-service", () => HealthCheckResult.Healthy("PDF service is running"));

// Add health check endpoint
app.MapHealthChecks("/health");
```

## Troubleshooting

### Common Issues:

1. **Chrome not found on Azure:**
   - Use the Dockerfile approach
   - Or use Azure Container Instances

2. **Memory issues:**
   - Implement browser pooling
   - Use smaller page sizes
   - Add memory monitoring

3. **Timeout issues:**
   - Increase timeout settings
   - Use async/await properly
   - Implement retry logic

### Debug Commands:

```bash
# Check if Chrome is installed
google-chrome --version

# Test PDF generation locally
dotnet run --project Warehousing.Api

# Check logs
docker logs your-container-name
```

## Cost Optimization

1. **Use Azure Container Instances** for better cost control
2. **Implement caching** to reduce PDF generation
3. **Use smaller container sizes** when possible
4. **Monitor usage** with Application Insights

## Security Considerations

1. **Validate HTML content** before processing
2. **Sanitize user input** in PDF generation
3. **Use HTTPS** for all communications
4. **Implement rate limiting** for PDF generation
5. **Add authentication** to print endpoints

## Next Steps

1. ✅ Deploy the API with PDF support
2. ✅ Test PDF generation
3. ✅ Update Angular app to use PDF service
4. ✅ Monitor performance and costs
5. ✅ Add more document types as needed









