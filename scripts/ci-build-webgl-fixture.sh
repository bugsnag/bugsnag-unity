#!/bin/bash -e

# === Validate Environment ===
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
BUILD_SCRIPT="./features/scripts/build_maze_runner.sh"
BUILD_DIR="features/fixtures/maze_runner/build"

# === Trigger WebGL Build ===
echo "🚧 Building WebGL fixture ($BUILD_TYPE)..."
"$BUILD_SCRIPT" "$BUILD_TYPE" webgl

# === Prepare Archive ===
(
  cd "$BUILD_DIR"

  if [ "$BUILD_TYPE" == "release" ]; then
    FIXTURE_NAME="Mazerunner"
    ZIP_NAME="WebGL-release-${UNITY_VERSION_SHORT}.zip"
  else
    FIXTURE_NAME="Mazerunner_dev"
    ZIP_NAME="WebGL-dev-${UNITY_VERSION_SHORT}.zip"
  fi

  echo "📦 Creating archive: $ZIP_NAME"
  zip -r "$ZIP_NAME" WebGL

  # === Validate Build Output ===
  if [ ! -f "WebGL/${FIXTURE_NAME}/index.html" ]; then
    echo "❌ index.html not found in WebGL/${FIXTURE_NAME}"
    exit 3
  fi

  echo "✅ WebGL build archive created: $ZIP_NAME"
)
