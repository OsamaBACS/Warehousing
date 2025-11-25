## Secure configuration and secrets management

Never check real credentials into `appsettings.json` or any other source file.  
Use the following patterns instead:

### Local development (.NET API)

1. Keep the committed `appsettings.json` empty (placeholders only).  
2. Store real values via [.NET user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):
   ```bash
   cd /media/osama/MyData/GitHub/Warehousing/Warehousing.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;"
   dotnet user-secrets set "Jwt:SecretKey" "your-long-random-secret"
   dotnet user-secrets set "AzureStorage:ConnectionString" "DefaultEndpointsProtocol=..."
   dotnet user-secrets set "AzureStorage:BaseUrl" "https://<account>.blob.core.windows.net/images"
   ```
3. User-secrets are stored outside the repo, so they cannot leak in commits.

### Azure App Service / production

1. In the Web App → *Configuration* → *Application settings*, add the same keys:
   - `ConnectionStrings:DefaultConnection`
   - `Jwt:SecretKey`
   - `AzureStorage:ConnectionString`
   - `AzureStorage:BaseUrl`
2. Restart the app so the new settings take effect.
3. Optional: store secrets in Azure Key Vault and reference them from App Service.

### Angular environment configuration

Angular already uses file replacements:
- `environment.ts` → local dev base URL
- `environment.prod.ts` → production base URL

To avoid manual edits before each deploy, keep the prod API URL in `environment.prod.ts` and run:

```bash
ng build --configuration production
```

If you need runtime configuration (e.g., multiple staging slots), load a JSON config from `assets` at boot instead of hardcoding.

### If a secret leaks

1. Remove it from the working tree.
2. Rotate/revoke the leaked credential in Azure/SQL.
3. Rewrite the git history (e.g., `git reset` or `git filter-repo`) so the secreted commit is not pushed.
4. Commit the sanitized file and push again.

GitHub Push Protection will block any commit that still contains high-entropy secrets—scrub them before pushing.

