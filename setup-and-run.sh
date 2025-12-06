#!/bin/bash

# Setup Xcode and run Location Tracker app

set -e

echo "🔧 Setting up Xcode and building Location Tracker..."
echo ""

# Use .NET 8.0 from user directory
export PATH="$HOME/.dotnet:$PATH"

# Check if Xcode is installed
if [ ! -d "/Applications/Xcode.app" ]; then
    echo "❌ Xcode not found at /Applications/Xcode.app"
    echo "Please wait for Xcode to finish installing from the App Store."
    exit 1
fi

echo "✅ Xcode found!"

# Configure xcode-select
echo "Configuring xcode-select..."
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer

# Accept Xcode license (if needed)
echo "Accepting Xcode license..."
sudo xcodebuild -license accept || echo "License already accepted or requires manual acceptance"

# Verify Xcode
echo ""
echo "Xcode version:"
xcodebuild -version

# Build and run the app
echo ""
echo "🚀 Building and running Location Tracker..."
echo ""

cd "$(dirname "$0")"
dotnet build -t:Run -f net8.0-maccatalyst

