#!/bin/bash -e

# === Validate Environment Variable ===
if [ -z "$UNITY_VERSION" ]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

# === Validate Arguments ===
if [[ $# -ne 1 ]]; then
  echo "❌ Usage: $0 <release|dev>"
  exit 2
fi

BUILD_TYPE=$1
UNITY_VERSION_SHORT="${UNITY_VERSION:0:4}"
BUILD_DIR="features/fixtures/maze_runner/build"
WEBGL_DIR="$BUILD_DIR/WebGL"

# === Unzip WebGL Fixture ===
echo "📦 Unzipping WebGL $BUILD_TYPE build..."
(
  cd "$BUILD_DIR"
  if [ "$BUILD_TYPE" == "release" ]; then
    ZIP_NAME="WebGL-release-${UNITY_VERSION_SHORT}.zip"
  else
    ZIP_NAME="WebGL-dev-${UNITY_VERSION_SHORT}.zip"
  fi

  unzip -q "$ZIP_NAME"

  if [ ! -d "$WEBGL_DIR" ]; then
    echo "❌ Expected WebGL directory not found after unzip: $WEBGL_DIR"
    exit 3
  fi
)

# === Run Maze Runner ===
echo "🚀 Running Maze Runner for WebGL (Firefox)..."
bundle install

# TODO: PLAT-8151 — Re-enable WebGL persistence tests when issue is resolved
bundle exec maze-runner --farm=local --browser=firefox \
  -e features/csharp/csharp_persistence.feature \
  features/csharp
