#!/bin/bash -e

# === Validate Inputs ===
if [ -z "$UNITY_VERSION" ]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

if [[ $# -ne 1 ]]; then
  echo "❌ Usage: $0 <release|dev>"
  exit 2
fi

BUILD_TYPE=$1
UNITY_VERSION_SHORT="${UNITY_VERSION:0:4}"
BUILD_DIR="features/fixtures/maze_runner/build"
APP_DIR="$BUILD_DIR/MacOS"

echo "📦 Extracting MacOS $BUILD_TYPE build..."

# === Extract App ===
(
  cd "$BUILD_DIR"
  ZIP_NAME="MacOS-${BUILD_TYPE}-${UNITY_VERSION_SHORT}.zip"
  unzip -q "$ZIP_NAME"

  if [ "$BUILD_TYPE" == "release" ]; then
    APP_PATH="$APP_DIR/Mazerunner.app"
  else
    APP_PATH="$APP_DIR/Mazerunner_dev.app"
  fi

  # Export for use outside the subshell
  echo "$APP_PATH"
) > .extracted_app_path

APP_PATH=$(cat .extracted_app_path)
rm .extracted_app_path

# === Validate App Path ===
if [ ! -d "$APP_PATH" ]; then
  echo "❌ Expected app not found at: $APP_PATH"
  exit 3
fi

# === Run Maze Runner ===
echo "🚀 Running Maze Runner with app: $APP_PATH"
bundle install
bundle exec maze-runner --app="$APP_PATH" --os=macos features/macos features/csharp
