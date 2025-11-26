#!/usr/bin/env python3
import ftplib
import os
import sys
from pathlib import Path

FTP_HOST = "warehousing.somee.com"
FTP_USER = "OsamaBACS"
FTP_PASS = "Bus1989@123"
FTP_PATH = "/www.warehousing.somee.com"
LOCAL_PATH = "Warehousing.Api/bin/Release/Publish"

def upload_file(ftp, local_path, remote_path):
    """Upload a single file to FTP server"""
    try:
        with open(local_path, 'rb') as file:
            ftp.storbinary(f'STOR {remote_path}', file)
        return True
    except Exception as e:
        print(f"Error uploading {local_path}: {e}")
        return False

def upload_directory(ftp, local_dir, remote_dir):
    """Recursively upload directory to FTP server"""
    print(f"Uploading {local_dir} to {remote_dir}...")
    
    for root, dirs, files in os.walk(local_dir):
        # Calculate relative path
        rel_root = os.path.relpath(root, local_dir)
        if rel_root == '.':
            remote_root = remote_dir
        else:
            remote_root = f"{remote_dir}/{rel_root}".replace('\\', '/')
        
        # Create remote directory
        try:
            ftp.cwd('/')
            for part in remote_root.strip('/').split('/'):
                if part:
                    try:
                        ftp.cwd(part)
                    except:
                        ftp.mkd(part)
                        ftp.cwd(part)
        except Exception as e:
            print(f"Warning: Could not create directory {remote_root}: {e}")
        
        # Upload files
        for file in files:
            if file.endswith(('.pdb', '.dbg')):
                continue  # Skip debug files
            local_file = os.path.join(root, file)
            remote_file = f"{remote_root}/{file}".replace('\\', '/')
            print(f"  Uploading: {file}")
            upload_file(ftp, local_file, remote_file)
    
    print(f"Completed uploading {local_dir}")

def main():
    if not os.path.exists(LOCAL_PATH):
        print(f"ERROR: Source directory not found: {LOCAL_PATH}")
        sys.exit(1)
    
    print("=== Starting FTP Deployment ===")
    print(f"Host: {FTP_HOST}")
    print(f"User: {FTP_USER}")
    print(f"Remote Path: {FTP_PATH}")
    print(f"Local Path: {LOCAL_PATH}")
    print("")
    
    try:
        # Connect to FTP server
        print("Connecting to FTP server...")
        ftp = ftplib.FTP(FTP_HOST)
        ftp.login(FTP_USER, FTP_PASS)
        ftp.set_pasv(True)
        print("Connected successfully!")
        
        # Change to remote directory
        try:
            ftp.cwd(FTP_PATH)
        except:
            print(f"Creating remote directory: {FTP_PATH}")
            parts = FTP_PATH.strip('/').split('/')
            ftp.cwd('/')
            for part in parts:
                if part:
                    try:
                        ftp.cwd(part)
                    except:
                        ftp.mkd(part)
                        ftp.cwd(part)
        
        # Upload files
        upload_directory(ftp, LOCAL_PATH, '.')
        
        # Close connection
        ftp.quit()
        print("")
        print("=== Deployment Complete ===")
        print(f"Files uploaded to: ftp://{FTP_HOST}{FTP_PATH}")
        
    except Exception as e:
        print(f"ERROR: Deployment failed: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
