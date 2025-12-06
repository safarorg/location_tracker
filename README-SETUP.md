# Quick Setup Guide

## While Xcode is Downloading

The Xcode download is in progress. Once it's complete:

## After Xcode Installation Completes

1. **Open Xcode once** (to complete the installation and accept agreements)
   - You can close it immediately after it opens

2. **Run the setup script:**
   ```bash
   ./setup-and-run.sh
   ```
   
   This will:
   - Configure xcode-select
   - Accept the Xcode license
   - Build and run your app

## Manual Steps (if script doesn't work)

```bash
# Use .NET 8.0
export PATH="$HOME/.dotnet:$PATH"

# Configure Xcode
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
sudo xcodebuild -license accept

# Build and run
dotnet build -t:Run -f net8.0-maccatalyst
```

## What to Expect

Once the app runs, you'll see a window with:
- Two input fields (Latitude and Longitude)
- A "Show on Map" button
- A map view

Enter coordinates like:
- San Francisco: `37.7749, -122.4194`
- New York: `40.7128, -74.0060`

The app will display a pin on the map at that location!

