# DuckDNS Updater

A simple Windows Forms application that automatically updates your DuckDNS domain with your current public IP address. It runs in the background and can minimize to system tray for seamless operation.

## Features
- ✅ Automatic IP monitoring and DuckDNS updates
- ✅ Configurable update intervals (1-1440 minutes)
- ✅ Manual update capability
- ✅ Auto-start functionality
- ✅ System tray integration with minimize to tray
- ✅ Real-time logging with colored output
- ✅ Configuration persistence
- ✅ Custom DuckDNS icon
- ✅ Single-file executable (no installation required)

## Quick Start

### Option 1: Download Standalone Executable (Recommended)
1. Download `DuckDNSUpdater-Standalone.exe` from releases
2. Run the executable directly - no installation needed!
3. Configure your DuckDNS subdomain and token
4. Click "Start" to begin automatic updates

### Option 2: Build from Source
1. Install .NET 9.0 SDK
2. Clone this repository
3. Run the build script:
   ```powershell
   .\build-standalone.ps1
   ```
4. Use the generated `DuckDNSUpdater-Standalone.exe`

## Usage
1. Enter your DuckDNS subdomain (without .duckdns.org)
2. Enter your DuckDNS token from your DuckDNS account
3. Set update interval (default: 5 minutes)
4. Click "Save Config" to persist settings
5. Click "Start" to begin automatic updates
6. Optionally enable "Minimize to Tray" for background operation

## Configuration
Settings are automatically saved in `duckdns_config.json`:
- Subdomain and token
- Update interval
- Auto-start preference
- Minimize to tray preference

## System Requirements
- Windows 10 or Windows 11
- Internet connection
- Valid DuckDNS account and token

*Note: The standalone executable includes .NET runtime - no separate installation required.*

## Build Commands

### Standard Build
```bash
dotnet build --configuration Release
```

### Create Standalone Executable
```bash
.\build-standalone.ps1
```

### Manual Standalone Build
```bash
dotnet publish DuckDNSUpdater.csproj --configuration Release --runtime win-x64 --self-contained true --output "standalone-build" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none
```
