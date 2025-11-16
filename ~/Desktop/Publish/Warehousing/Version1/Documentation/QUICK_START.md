# Quick Start Guide - Version 1

## Fast Deployment (5 Steps)

### 1. Install Prerequisites
- Install .NET 9.0 Hosting Bundle
- Ensure SQL Server is running

### 2. Create Database (Optional - Auto-created if permissions allow)
```sql
CREATE DATABASE WarehousingDB;
```

### 3. Update Connection String
Edit `Backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=WarehousingDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### 4. Deploy to IIS
- Copy `Backend` folder to: `C:\inetpub\wwwroot\Warehousing\Version1\`
- Create IIS Website pointing to this folder
- Set Application Pool to "No Managed Code"
- Start the website

### 5. Access Application
- Open: `http://localhost` (or your configured URL)
- Login: `admin` / `Admin@123`

## Default Credentials
- **Username**: `admin`
- **Password**: `Admin@123`

**⚠️ IMPORTANT: Change password immediately after first login!**

## File Structure
```
Backend/
├── wwwroot/          # Angular frontend (served automatically)
├── appsettings.json  # Configuration (UPDATE THIS!)
└── [other files]     # Application binaries
```

## Need Help?
See `DEPLOYMENT_GUIDE.md` for detailed instructions.


