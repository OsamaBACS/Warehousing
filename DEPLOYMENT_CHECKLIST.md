# Deployment Checklist for Somee.com

## ✅ Completed Preparations

1. **Database Connection String** - Updated in appsettings.json
2. **CORS Settings** - Added www.warehousing.somee.com
3. **JWT Configuration** - Updated for production domain
4. **Angular Production Build** - Built and copied to wwwroot/browser
5. **.NET Application** - Built and published to bin/Release/Publish

## 📦 Files Ready for Deployment

### Location: `/media/osama/MyData/GitHub/Warehousing/Warehousing.Api/bin/Release/Publish/`

This folder contains:
- All .NET DLL files (69 DLL files)
- appsettings.json with production connection string
- wwwroot folder with Angular application
- All required dependencies

## 🚀 FTP Deployment Steps

### 1. Connect to FTP
- **Host:** warehousing.somee.com
- **Port:** 21 (default FTP)
- **Username:** OsamaBACS
- **Password:** Bus1989@123
- **Path:** /www.warehousing.somee.com

### 2. Files to Upload

Upload ALL files from:
```
Warehousing.Api/bin/Release/Publish/
```

**Important:** Make sure to:
- Upload in binary mode (not ASCII)
- Preserve folder structure
- Upload wwwroot folder completely
- Upload appsettings.json

### 3. After Upload

1. **Verify database connection** - Application will auto-migrate on first run
2. **Check application logs** - Look for any startup errors
3. **Test API endpoints** - Visit http://www.warehousing.somee.com/api
4. **Test Angular app** - Visit http://www.warehousing.somee.com

## ⚠️ Important Notes

1. **Database Auto-Migration**: The application will automatically apply migrations on startup
2. **Data Seeding**: Initial data (statuses, roles, permissions) will be seeded automatically
3. **First Admin User**: Will be created automatically (username: admin, password: Admin@123)
4. **Resources Folder**: Ensure Resources folder exists for file uploads

## 🔍 Verification Checklist

- [ ] All files uploaded successfully
- [ ] Database migrations applied (check logs)
- [ ] Application starts without errors
- [ ] API endpoints responding
- [ ] Angular application loads
- [ ] Login page accessible
- [ ] Can login with admin credentials

## 🐛 Troubleshooting

If the application doesn't start:
1. Check Somee.com control panel for error logs
2. Verify connection string is correct
3. Check if database is accessible
4. Ensure all DLL files are uploaded

## 📝 Configuration Summary

- **Database:** WarehousingDB.mssql.somee.com
- **Connection String:** Already configured in appsettings.json
- **Domain:** www.warehousing.somee.com
- **JWT Secret:** JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr

