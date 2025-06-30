#!/bin/bash -e

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
ZIP_NAME="MacOS-${BUILD_TYPE}-${UNITY_VERSION_SHORT}.zip"

# === Build MacOS and WebGL Fixtures ===
echo "🚧 Building MacOS and WebGL fixtures for $BUILD_TYPE..."
./features/scripts/build_maze_runner.sh "$BUILD_TYPE" macos

# === Create ZIP archive ===
if [ ! -d "$BUILD_DIR/MacOS" ]; then
  echo "❌ Error: MacOS build directory not found at $BUILD_DIR/MacOS"
  exit 3
fi

echo "📦 Creating archive: $ZIP_NAME"
(
  cd "$BUILD_DIR"
  zip -r "$ZIP_NAME" MacOS
)

echo "✅ Archive created at $BUILD_DIR/$ZIP_NAME"
