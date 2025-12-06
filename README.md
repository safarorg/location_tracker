# Location Tracker

A simple macOS app to display coordinates on a map.

## Features

- Enter latitude and longitude coordinates
- Display location on an interactive map
- Simple, clean interface

## Requirements

- macOS 13.1 or later
- .NET 8.0 SDK
- .NET MAUI workload

## Setup

```bash
# Install .NET MAUI workload (if not already installed)
sudo dotnet workload install maui

# Restore packages
dotnet restore

# Build and run
dotnet build -t:Run -f net8.0-maccatalyst
```

## Usage

1. Enter latitude (e.g., `37.7749`)
2. Enter longitude (e.g., `-122.4194`)
3. Click "Show on Map"
4. The map will center on the location with a pin

## Example Coordinates

- San Francisco: `37.7749, -122.4194`
- New York: `40.7128, -74.0060`
- London: `51.5074, -0.1278`

