# Warehousing Application - Deployment Guide (Version 1)

## Overview
This deployment package contains both the backend (.NET API) and frontend (Angular) applications ready for IIS deployment.

## Folder Structure
```
Version1/
├── Backend/          # .NET Core API application
├── Frontend/         # Angular production build (standalone)
└── Documentation/    # Deployment documentation
```

## Prerequisites

### Server Requirements
- Windows Server with IIS 10.0 or later
- .NET 9.0 Runtime (Hosting Bundle)
- SQL Server 2019 or later
- SQL Server Management Studio (optional, for database management)

### Software Installation

1. **Install .NET 9.0 Hosting Bundle**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Install: `dotnet-hosting-9.0.x-win.exe`
   - Restart IIS after installation

2. **Install SQL Server** (if not already installed)
   - Ensure SQL Server is running
   - Note the server name and credentials

## Deployment Steps

### Step 1: Database Setup

#### Option A: Automatic (Recommended)
The application will automatically create the database and apply migrations on first startup if:
- SQL Server is accessible
- Connection string is correct
- SQL user has CREATE DATABASE permission

#### Option B: Manual Database Creation
1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Create a new database named `WarehousingDB` (or your preferred name)
4. Update the connection string in `appsettings.json` accordingly

### Step 2: Configure Connection String

1. Navigate to `Backend/appsettings.json` (or create `appsettings.Production.json`)
2. Update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=WarehousingDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

**Important:** For production, create `appsettings.Production.json` with production settings.

### Step 3: IIS Configuration

1. **Open IIS Manager**
   - Press `Win + R`, type `inetmgr`, press Enter

2. **Create Application Pool**
   - Right-click "Application Pools" → "Add Application Pool"
   - Name: `WarehousingAppPool`
   - .NET CLR Version: **No Managed Code** (for .NET Core)
   - Managed Pipeline Mode: **Integrated**
   - Click OK

3. **Configure Application Pool**
   - Select `WarehousingAppPool`
   - Click "Advanced Settings"
   - Set:
     - **Identity**: ApplicationPoolIdentity (or custom account with DB permissions)
     - **Start Mode**: AlwaysRunning (optional, for better performance)

4. **Create Website**
   - Right-click "Sites" → "Add Website"
   - Site name: `Warehousing`
   - Application pool: `WarehousingAppPool`
   - Physical path: `C:\inetpub\wwwroot\Warehousing\Version1\Backend` (or your deployment path)
   - Binding:
     - Type: `http`
     - IP address: `All Unassigned` (or specific IP)
     - Port: `80` (or your preferred port)
     - Host name: (leave empty or set your domain)
   - Check "Start website immediately"
   - Click OK

5. **Configure Website Permissions**
   - Right-click the website → "Edit Permissions"
   - Security tab → Add:
     - `IIS_IUSRS` (Read & Execute)
     - `IIS AppPool\WarehousingAppPool` (Read & Execute)
   - Click OK

### Step 4: Copy Files to IIS

1. Copy the entire `Backend` folder to your IIS deployment location:
   ```
   C:\inetpub\wwwroot\Warehousing\Version1\Backend
   ```

2. Ensure the `wwwroot` folder contains the Angular files (already included in Backend folder)

### Step 5: Configure Static Files

The Angular application is already configured to be served from the `wwwroot` folder. The backend will automatically serve:
- Angular application from root URL
- API endpoints from `/api/*` routes
- Static resources from `/Resources/*` (if using local file storage)

### Step 6: Test Deployment

1. **Start the Website**
   - In IIS Manager, select your website
   - Click "Start" in the Actions panel

2. **Check Application Logs**
   - Navigate to: `Backend\logs` (if logging is configured)
   - Or check Windows Event Viewer → Application logs

3. **Access the Application**
   - Open browser: `http://localhost` (or your configured URL)
   - You should see the Angular login page

4. **Test API**
   - Navigate to: `http://localhost/api/health` (if health check endpoint exists)
   - Or: `http://localhost/swagger` (if Swagger is enabled in production)

### Step 7: Default Login Credentials

After first deployment, use these credentials:
- **Username**: `admin`
- **Password**: `Admin@123`

**Important:** Change the default password immediately after first login!

## Configuration Files

### appsettings.json
Main configuration file. Contains:
- Connection strings
- JWT settings
- Logging configuration

### appsettings.Production.json (Create if needed)
Production-specific settings that override `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_PRODUCTION_CONNECTION_STRING"
  },
  "Jwt": {
    "SecretKey": "YOUR_PRODUCTION_SECRET_KEY",
    "Issuer": "YOUR_PRODUCTION_ISSUER",
    "Audience": "YOUR_PRODUCTION_AUDIENCE"
  }
}
```

## Troubleshooting

### Application Won't Start

1. **Check .NET Runtime**
   - Verify .NET 9.0 Hosting Bundle is installed
   - Restart IIS: `iisreset` in Command Prompt (as Administrator)

2. **Check Application Pool**
   - Ensure it's started
   - Check for errors in Event Viewer

3. **Check Database Connection**
   - Verify SQL Server is running
   - Test connection string
   - Check SQL Server authentication

4. **Check File Permissions**
   - Ensure IIS_IUSRS has read access
   - Check Application Pool identity permissions

### Database Migration Errors

1. **Check SQL Server Permissions**
   - User needs: CREATE DATABASE, CREATE TABLE, ALTER TABLE permissions

2. **Check Connection String**
   - Verify server name, database name, credentials
   - Test connection in SQL Server Management Studio

3. **Manual Migration** (if automatic fails)
   ```bash
   cd Backend
   dotnet ef database update --project ../Warehousing.Data --startup-project .
   ```

### Angular App Not Loading

1. **Check wwwroot Folder**
   - Ensure Angular files are in `Backend/wwwroot/`
   - Verify `index.html` exists

2. **Check Static Files Middleware**
   - Verify `app.UseStaticFiles()` is in `Program.cs`

3. **Check Browser Console**
   - Open Developer Tools (F12)
   - Check for JavaScript errors
   - Check Network tab for failed requests

## Security Recommendations

1. **Change Default Password**
   - Immediately after first login

2. **Update JWT Secret Key**
   - Generate a strong secret key
   - Update in `appsettings.Production.json`

3. **Use HTTPS**
   - Install SSL certificate
   - Configure HTTPS binding in IIS
   - Update connection strings to use HTTPS

4. **Restrict Database Access**
   - Use least privilege principle
   - Create dedicated SQL user with minimal permissions

5. **Enable Windows Authentication** (Optional)
   - For internal applications
   - Configure in IIS and application

## Backup and Maintenance

### Database Backup
- Schedule regular SQL Server backups
- Test restore procedures

### Application Backup
- Backup the `Backend` folder regularly
- Keep version history (Version1, Version2, etc.)

### Logs
- Monitor application logs
- Set up log rotation
- Archive old logs

## Support

For issues or questions:
1. Check application logs
2. Check Windows Event Viewer
3. Review this documentation
4. Contact development team

## Version Information

- **Version**: 1.0
- **Build Date**: $(date)
- **.NET Version**: 9.0
- **Angular Version**: 20.0


