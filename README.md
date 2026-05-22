# Ssomero — Academic Management System

Cross-platform .NET MAUI mobile/desktop app for lecturer–student academic management.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 17.12+ with the **.NET MAUI** workload
- Android SDK 21+ (for Android builds)
- Xcode 15+ (for iOS/macOS builds)

## Quick Start

```bash
# Clone
git clone https://github.com/your-org/ssomero.git
cd ssomero

# Restore
dotnet restore

# Run on Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0

# Run on Android emulator
dotnet build -t:Run -f net10.0-android
```

## Configuration

API settings are in `Ssomero/appsettings.json` (production) and `Ssomero/appsettings.Development.json` (debug).

```json
{
  "ApiSettings": {
    "BaseUrl": "https://api.ssomero.com/api/",
    "TimeoutSeconds": 30
  }
}
```

The Development config is only included in Debug builds.

## Project Structure

```
Ssomero/
??? Configuration/     # Strongly-typed settings (ApiSettings)
??? Components/        # Reusable XAML controls (CardView)
??? Converters/        # IValueConverter implementations
??? Interfaces/        # Service contracts
??? Models/            # DTOs (CourseDto, AuthResponseDto, etc.)
??? Services/          # Service implementations (API, Auth, Token)
??? ViewModels/        # MVVM ViewModels
??? Views/             # XAML pages organized by feature
?   ??? Auth/
?   ??? Courses/
?   ??? Dashboard/
?   ??? Announcements/
??? Resources/         # Fonts, images, styles, colors
??? appsettings.json
??? MauiProgram.cs     # DI composition root
```

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for details.

## Testing

```bash
cd Ssomero.Tests
dotnet test
```

## Building for Release

```bash
# Android AAB
dotnet publish -f net10.0-android -c Release

# iOS
dotnet publish -f net10.0-ios -c Release
```

## License

Proprietary — All rights reserved.
