#!/bin/bash
set -e

FTP_HOST="warehousing.somee.com"
FTP_USER="OsamaBACS"
FTP_PASS="Bus1989@123"
FTP_PATH="/www.warehousing.somee.com"
LOCAL_PATH="Warehousing.Api/bin/Release/Publish"

echo "=== Starting FTP Deployment ==="
echo "Source: $LOCAL_PATH"
echo "Destination: $FTP_HOST$FTP_PATH"
echo ""

# Check if lftp is available
if ! command -v lftp &> /dev/null; then
    echo "ERROR: lftp is not installed."
    echo "Please install it with: sudo apt-get install lftp"
    exit 1
fi

# Check if source directory exists
if [ ! -d "$LOCAL_PATH" ]; then
    echo "ERROR: Source directory not found: $LOCAL_PATH"
    exit 1
fi

echo "Uploading files..."
lftp -c "
set ftp:ssl-allow no
set ftp:passive-mode yes
open -u $FTP_USER,$FTP_PASS $FTP_HOST
cd $FTP_PATH
mirror -R --delete --verbose --exclude-glob='*.pdb' --exclude-glob='*.dbg' $LOCAL_PATH .
bye
"

echo ""
echo "=== Deployment Complete ==="
echo "Files uploaded to: ftp://$FTP_HOST$FTP_PATH"
echo ""
echo "Next steps:"
echo "1. Visit http://www.warehousing.somee.com to verify"
echo "2. Check application logs in Somee.com control panel"
echo "3. Test login with admin credentials"
