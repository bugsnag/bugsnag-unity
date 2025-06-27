#!/usr/bin/env bash
set -euo pipefail

# === Validate environment ===
if [[ -z "${UNITY_VERSION:-}" ]]; then
  echo "❌ UNITY_VERSION must be set"
  exit 1
fi

if [[ $# -lt 1 ]]; then
  echo "❌ Build type must be specified: 'release' or 'dev'"
  exit 1
fi

BUILD_TYPE="$1"

if [[ "$BUILD_TYPE" != "dev" && "$BUILD_TYPE" != "release" ]]; then
  echo "❌ Invalid build type specified: must be 'release' or 'dev'"
  exit 1
fi

# === Paths and Constants ===
UNITY_EXEC="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FIXTURE_DIR="$SCRIPT_DIR/../fixtures"
PROJECT_PATH="$FIXTURE_DIR/maze_runner"
LOG_FILE="unity.log"
UNITY_CLI_ARGS="-quit -nographics -batchmode -logFile $LOG_FILE -buildTarget iOS"

cd "$FIXTURE_DIR"

# === Select build method ===
BUILD_METHOD="Builder.Ios$(echo "$BUILD_TYPE" | awk '{print toupper(substr($0,1,1)) tolower(substr($0,2))}')"

# === Build Xcode project (twice, due to Unity IL2CPP linker flag bug) ===
for i in {1..2}; do
  echo "🔁 Unity iOS build pass $i with method $BUILD_METHOD..."
  "$UNITY_EXEC" $UNITY_CLI_ARGS -projectPath "$PROJECT_PATH" -executeMethod "$BUILD_METHOD"
done

echo "✅ iOS Xcode project generated successfully (2-pass workaround complete)"
