#!/bin/bash

# Angular UI Deployment Script for Azure Web App
# Usage: ./deploy-angular.sh

set -e  # Exit on error

echo "🚀 Starting Angular deployment..."

# Navigate to Angular project
cd "$(dirname "$0")/Warehousing.UI"

# Build Angular for production
echo "📦 Building Angular application..."
npm run build -- --configuration production

# Navigate to build output
cd dist/Warehousing.UI/browser

# Create zip file
echo "📝 Creating deployment package..."
rm -f ~/angular.zip
zip -r ~/angular.zip . > /dev/null

# Deploy to Azure
echo "☁️  Deploying to Azure Web App..."
az webapp deploy \
  --resource-group WarehousingWebsite \
  --name WareHousingUI \
  --src-path ~/angular.zip \
  --type zip

echo "✅ Deployment completed successfully!"
echo "🌐 Visit: http://warehousingui-cpbyb7d8hadwgwcp.canadacentral-01.azurewebsites.net"



