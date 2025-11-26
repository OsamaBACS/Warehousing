# Correct Runtimes Folder Structure

The `runtimes` folder should have this structure:

```
runtimes/
├── win/
│   └── lib/
│       └── net6.0/
│           ├── Microsoft.Data.SqlClient.dll
│           ├── Microsoft.Win32.SystemEvents.dll
│           ├── System.Drawing.Common.dll
│           ├── System.Runtime.Caching.dll
│           ├── System.Security.Cryptography.ProtectedData.dll
│           └── System.Windows.Extensions.dll
├── unix/
│   └── lib/
│       └── net6.0/
│           ├── Microsoft.Data.SqlClient.dll
│           └── System.Drawing.Common.dll
├── win-arm/
│   └── native/
│       └── Microsoft.Data.SqlClient.SNI.dll
├── win-arm64/
│   └── native/
│       └── Microsoft.Data.SqlClient.SNI.dll
├── win-x64/
│   └── native/
│       └── Microsoft.Data.SqlClient.SNI.dll
└── win-x86/
    └── native/
        └── Microsoft.Data.SqlClient.SNI.dll
```

## Problem in Your Server

The FTP upload created nested `runtimes/runtimes/runtimes/...` folders which is incorrect.

## Solution

1. **Delete the entire nested runtimes structure** via File Manager
2. **Create the correct structure manually** in File Manager
3. **Upload files** to the correct locations

Or simply delete the entire `runtimes` folder and recreate it correctly.

## Quick Fix (Windows-specific files only)

For Windows hosting, you only need:
- `runtimes/win/lib/net6.0/` - Contains Windows-specific DLLs

The other folders (unix, win-arm, etc.) are optional for Windows IIS hosting.
