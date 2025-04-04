# ScanApp - File Scanner Application for cross- Platform designed for windows and macOS

A cross-platform .NET Core console application for scanning directories, calculating file hashes, and maintaining a scan history database.

## Features

- **Multi-threaded Scanning**: Processes files in parallel using all available CPU cores
- **Hash Calculation**: Computes MD5, SHA1, and SHA256 hashes for each file
- **Database Storage**: SQLite database tracks file metadata and scan history
- **Cross-Platform Support**: Works on Windows, Linux, and macOS
- **Comprehensive Logging**: File and console logging with different verbosity levels
- **Smart Scanning**:
  - Skips unchanged files between scans
  - Platform-specific file handling (hidden files, system directories)
  - Error handling and retry mechanisms

## Tech Stack

- **.NET 8** - Cross-platform runtime
- **Dapper** - Lightweight ORM for SQLite
- **Microsoft.Data.Sqlite** - SQLite database provider
- **Microsoft.Extensions.Logging** - Logging infrastructure

## Installation

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Clone & Build
```bash
git clone https://github.com/your-repo/ScanApp.git
cd ScanApp
dotnet restore
dotnet build```

### Publish command for windows
```bash
dotnet publish -c Release -r win-x64 --self-contained```


### Publish command for macOS
```bash
dotnet publish -c Release -r osx-x64 --self-contained```



### For directly using publish.zip is added in git which as both widnows and macOS 
For windows usage was simple inside win-x64 
```bash
ScanApp G:\YourPath\ ```

for macos I had to do extra steps inside osx-x64 folder 
```bash
chmod +x ScanApp
codesign --force --deep --sign - ./ScanApp
xattr -dr com.apple.quarantine .
./ScanApp "/Users/yourname/Downloads"

 ```
