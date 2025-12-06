# Location Tracker

A native macOS desktop application built with C# and .NET MAUI that tracks location coordinates, saves them to an SQLite database, and visualizes them as a heat map using the .NET MAUI Maps component.

## Features

- **Manual Coordinate Input**: Enter latitude and longitude coordinates manually
- **SQLite Database**: All locations are persisted to a local SQLite database
- **Heat Map Visualization**: Displays locations as blue circles on an interactive map
- **Pre-populated Data**: Automatically loads 20 default coordinates around San Francisco on first launch
- **Real-time Updates**: Location count updates automatically
- **Clear Data**: Option to clear all stored locations

## Requirements

- macOS 13.1 or later
- .NET 8.0 SDK
- .NET MAUI workload
- Xcode (for building macOS Catalyst apps)

## Setup

### Prerequisites

1. Install .NET 8.0 SDK:
   ```bash
   # Using Homebrew
   brew install --cask dotnet-sdk
   
   # Or download from https://dotnet.microsoft.com/download
   ```

2. Install .NET MAUI workload:
   ```bash
   dotnet workload install maui
   ```

3. Install Xcode from the App Store (required for macOS Catalyst development)

4. Configure Xcode:
   ```bash
   sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
   sudo xcodebuild -license accept
   ```

### Build and Run

```bash
# Restore packages
dotnet restore

# Build and run
dotnet build -t:Run -f net8.0-maccatalyst

# Or just run
dotnet run -f net8.0-maccatalyst
```

## Usage

1. **Launch the app**: The app automatically loads 20 default coordinates around San Francisco on first launch
2. **View the heat map**: Blue circles represent location density on the map
3. **Add new coordinates**: 
   - Enter latitude (e.g., `37.7749`)
   - Enter longitude (e.g., `-122.4194`)
   - Click "Add" to save and display the location
4. **Clear all data**: Click "Clear" to remove all stored locations

## Technical Details

### Architecture

- **Framework**: .NET MAUI (Multi-platform App UI)
- **Platform**: macOS Catalyst
- **Database**: SQLite (using `sqlite-net-pcl`)
- **Map Component**: Microsoft.Maui.Controls.Maps
- **Heat Map**: Custom implementation using `Circle` overlays

### Project Structure

```
LocationTracker/
├── MainPage.cs              # Main UI and logic
├── Models/
│   └── LocationPoint.cs    # Data model for locations
├── Services/
│   ├── DatabaseService.cs  # SQLite database operations
│   └── LocationService.cs  # Location tracking service (currently unused)
├── Controls/
│   └── HeatMapOverlay.cs   # Heat map visualization logic
└── Platforms/
    └── MacCatalyst/
        ├── Info.plist      # App manifest (permissions, metadata)
        └── Entitlements.plist # App capabilities
```

### Manifest Files

- **`Platforms/MacCatalyst/Info.plist`**: Main manifest file containing app metadata and permissions
- **`Platforms/MacCatalyst/Entitlements.plist`**: App capabilities and entitlements (network access, etc.)

## Example Coordinates

- San Francisco: `37.7749, -122.4194`
- New York: `40.7128, -74.0060`
- London: `51.5074, -0.1278`
- Tokyo: `35.6762, 139.6503`

## Notes

- All locations are stored locally in SQLite database
- Heat map circles are displayed in blue color
- The app clears and reloads default coordinates on each launch to ensure fresh data
- Map automatically centers on the average of all locations
