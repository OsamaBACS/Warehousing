#!/bin/bash

# API Deployment Script for Azure Web App
# Usage: ./deploy-api.sh

set -e  # Exit on error

echo "🚀 Starting API deployment..."

# Navigate to project root
cd "$(dirname "$0")"

# Publish API
echo "📦 Publishing API..."
dotnet publish Warehousing.Api/Warehousing.Api.csproj -c Release -o ./publish

# Create zip file
echo "📝 Creating deployment package..."
cd publish
rm -f ~/api.zip
zip -r ~/api.zip . > /dev/null
cd ..

# Deploy to Azure
echo "☁️  Deploying to Azure Web App..."
az webapp deploy \
  --resource-group WarehousingWebsite \
  --name Warehouse \
  --src-path ~/api.zip \
  --type zip

echo "✅ Deployment completed successfully!"
echo "🌐 API URL: https://warehouse-gfgce9dheeecfgek.canadacentral-01.azurewebsites.net"


