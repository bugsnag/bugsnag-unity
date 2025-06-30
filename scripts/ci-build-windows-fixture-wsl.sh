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
ZIP_PREFIX="Windows-${BUILD_TYPE}-${UNITY_VERSION_SHORT}.zip"

# === Build Windows Fixture via WSL ===
echo "🛠️  Building Windows fixture via WSL..."
(
  cd features/scripts
  ./build_maze_runner.sh "$BUILD_TYPE" wsl
)

# === Archive the Build Output ===
echo "📦 Zipping build directory as $ZIP_PREFIX..."
(
  cd features/fixtures/maze_runner/build
  zip -r "$ZIP_PREFIX" Windows
)

echo "✅ Successfully created: $ZIP_PREFIX"
