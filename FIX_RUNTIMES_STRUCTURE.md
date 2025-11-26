# Fix Runtimes Folder Structure

## ❌ Current Problem (WRONG):
```
runtimes/
  runtimes/          ← NESTED (WRONG!)
    runtimes/        ← NESTED (WRONG!)
      unix/
      win-arm/
      ...
```

## ✅ Correct Structure Should Be (FLAT):
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
├── unix/           (optional for Windows hosting)
├── win-arm/        (optional)
├── win-arm64/      (optional)
├── win-x64/        (optional - native DLLs)
└── win-x86/        (optional)
```

## 📋 Step-by-Step Fix:

### Step 1: Delete Nested Structure
In Somee.com File Manager:
1. Navigate to `www.warehousing.somee.com/runtimes`
2. Delete the ENTIRE `runtimes` folder (it's all nested incorrectly)

### Step 2: Create Correct Structure
In Somee.com File Manager:
1. Create folder: `runtimes`
2. Inside `runtimes`, create: `win`
3. Inside `win`, create: `lib`
4. Inside `lib`, create: `net6.0`

Final path: `runtimes/win/lib/net6.0/`

### Step 3: Upload Files
Upload these 6 DLL files to `runtimes/win/lib/net6.0/`:
- Microsoft.Data.SqlClient.dll
- Microsoft.Win32.SystemEvents.dll
- System.Drawing.Common.dll
- System.Runtime.Caching.dll
- System.Security.Cryptography.ProtectedData.dll
- System.Windows.Extensions.dll

Source location: `Warehousing.Api/bin/Release/Publish/runtimes/win/lib/net6.0/`

## ⚠️ Important Note

For Windows IIS hosting (Somee.com), you ONLY need:
- `runtimes/win/lib/net6.0/` folder with the 6 DLL files

The other folders (unix, win-arm, etc.) are NOT needed for Windows hosting.
