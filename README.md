# DuckDNS Updater

## 1. Description
Because I can't find any DuckDNS updater client for Windows so I build one for myself.

This is a simple Windows Forms application that automatically updates your DuckDNS domain with your current public IP address. It runs in the background and can minimize to system tray for seamless operation.

## 2. Build from Source Guide

### Prerequisites
- .NET 9.0 or later
- Windows 10/11
- Visual Studio 2022 or VS Code with C# extension

### Steps
1. Clone or download the source code
2. Open terminal/command prompt in project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

### Alternative: Visual Studio
1. Open `DuckDNSUpdater.sln` in Visual Studio
2. Build Solution (Ctrl+Shift+B)
3. Start Debugging (F5) or Start Without Debugging (Ctrl+F5)

## 3. Features
- ✅ Automatic IP monitoring and DuckDNS updates
- ✅ Configurable update intervals (1-1440 minutes)
- ✅ Manual update capability
- ✅ Auto-start functionality
- ✅ System tray integration with minimize to tray
- ✅ Real-time logging with colored output
- ✅ Configuration persistence
- ✅ Clean and simple user interface

## 4. Usage
1. Enter your DuckDNS subdomain (without .duckdns.org)
2. Enter your DuckDNS token from your account
3. Set update interval (default: 5 minutes)
4. Click "Save Config" to persist settings
5. Click "Start" to begin automatic updates
6. Optionally enable "Minimize to Tray" for background operation

## 5. Configuration
The application saves settings in `duckdns_config.json` in the same directory. This includes:
- Subdomain and token
- Update interval
- Auto-start preference
- Minimize to tray preference

## 6. System Requirements
- Windows 10 or Windows 11
- .NET 9.0 Runtime
- Internet connection
- Valid DuckDNS account and token

## 7. Dependencies
- Newtonsoft.Json (for configuration management)
- .NET Windows Forms
- System.Net.Http (for API calls)

## 8. License
This project is provided as-is for personal use. Feel free to modify and distribute.
