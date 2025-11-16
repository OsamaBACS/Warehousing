# Deployment Checklist - Version 1

Use this checklist to ensure a successful deployment.

## Pre-Deployment

- [ ] .NET 9.0 Hosting Bundle installed on server
- [ ] SQL Server installed and running
- [ ] SQL Server credentials available
- [ ] IIS installed and configured
- [ ] Server has sufficient disk space (minimum 500MB)

## Database Setup

- [ ] Database created (or auto-creation verified)
- [ ] Connection string updated in `appsettings.json`
- [ ] SQL user has required permissions:
  - [ ] CREATE DATABASE
  - [ ] CREATE TABLE
  - [ ] ALTER TABLE
  - [ ] INSERT, UPDATE, DELETE, SELECT

## IIS Configuration

- [ ] Application Pool created:
  - [ ] Name: `WarehousingAppPool`
  - [ ] .NET CLR Version: **No Managed Code**
  - [ ] Managed Pipeline Mode: **Integrated**
  - [ ] Identity configured (ApplicationPoolIdentity or custom)

- [ ] Website created:
  - [ ] Site name: `Warehousing`
  - [ ] Physical path: `C:\inetpub\wwwroot\Warehousing\Version1\Backend`
  - [ ] Application Pool: `WarehousingAppPool`
  - [ ] Binding configured (HTTP/HTTPS)
  - [ ] Port configured (default: 80)

- [ ] Permissions set:
  - [ ] IIS_IUSRS (Read & Execute)
  - [ ] Application Pool Identity (Read & Execute)

## File Deployment

- [ ] Backend files copied to IIS directory
- [ ] Angular files present in `wwwroot` folder
- [ ] `appsettings.json` updated with production values
- [ ] `appsettings.Production.json` created (if needed)

## Configuration

- [ ] Connection string verified
- [ ] JWT secret key updated (production)
- [ ] JWT issuer/audience updated (production)
- [ ] Logging level configured appropriately
- [ ] CORS settings verified (if needed)

## Testing

- [ ] Website starts without errors
- [ ] Application accessible via browser
- [ ] Angular frontend loads correctly
- [ ] API endpoints respond (check `/api/health` or similar)
- [ ] Database connection successful
- [ ] Login page appears
- [ ] Can login with default credentials:
  - [ ] Username: `admin`
  - [ ] Password: `Admin@123`

## Post-Deployment

- [ ] Default admin password changed
- [ ] Application logs reviewed (no critical errors)
- [ ] Database migrations applied successfully
- [ ] Seeding data verified (check database tables)
- [ ] Backup strategy implemented
- [ ] Monitoring configured (optional)

## Security

- [ ] Default password changed
- [ ] JWT secret key is strong and unique
- [ ] HTTPS configured (if applicable)
- [ ] Firewall rules configured
- [ ] SQL Server access restricted
- [ ] File permissions set correctly

## Documentation

- [ ] Deployment guide reviewed
- [ ] Connection strings documented
- [ ] Credentials stored securely
- [ ] Backup procedures documented

## Sign-off

- [ ] Deployment completed by: ________________
- [ ] Date: ________________
- [ ] Tested by: ________________
- [ ] Approved by: ________________

## Notes

_Add any deployment-specific notes here:_





