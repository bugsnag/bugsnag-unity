#!/bin/bash -e

# === Validate Required Environment Variable ===
if [ -z "$UNITY_VERSION" ]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

# === Validate Input Arguments ===
if [[ $# -ne 1 ]]; then
  echo "❌ Usage: $0 <release|dev>"
  exit 2
fi

BUILD_TYPE=$1
UNITY_VERSION_SHORT="${UNITY_VERSION:0:4}"
BUILD_DIR="features/fixtures/maze_runner/build"
WINDOWS_DIR="$BUILD_DIR/Windows"

# === Navigate to Build Directory ===
cd "$BUILD_DIR"

# === Unzip Correct Windows Fixture ===
echo "📦 Unzipping Windows $BUILD_TYPE build..."
if [ "$BUILD_TYPE" == "release" ]; then
  ZIP_FILE="Windows-release-${UNITY_VERSION_SHORT}.zip"
  APP_NAME="Mazerunner.exe"
else
  ZIP_FILE="Windows-dev-${UNITY_VERSION_SHORT}.zip"
  APP_NAME="Mazerunner_dev.exe"
fi

unzip -q "$ZIP_FILE"

APP_PATH="$WINDOWS_DIR/$APP_NAME"

if [ ! -f "$APP_PATH" ]; then
  echo "❌ App executable not found at: $APP_PATH"
  exit 3
fi

# === Navigate Back to Repo Root ===
cd ../../../..

# === Run Maze Runner ===
echo "🚀 Running Maze Runner for Windows..."
bundle install
bundle exec maze-runner --app="$APP_PATH" --os=windows features/csharp
