#!/usr/bin/env bash
set -euo pipefail

# === Check environment variables ===
if [[ -z "${UNITY_VERSION:-}" ]]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

# === Validate arguments ===
if [[ $# -lt 1 ]]; then
  echo "❌ Build type must be specified: 'release' or 'dev'"
  exit 1
fi

BUILD_TYPE="$1"

if [[ "$BUILD_TYPE" != "dev" && "$BUILD_TYPE" != "release" ]]; then
  echo "❌ Invalid build type: must be 'release' or 'dev'"
  exit 1
fi

# === Constants ===
UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FIXTURE_DIR="$SCRIPT_DIR/../fixtures"
PROJECT_PATH="$FIXTURE_DIR/maze_runner"
LOG_FILE="build_android_apk.log"
UNITY_CLI_ARGS="-quit -batchmode -nographics -logFile $LOG_FILE -buildTarget Android"

cd "$FIXTURE_DIR"

# === Set build method and APK filenames ===
if [[ "$BUILD_TYPE" == "dev" ]]; then
  BUILD_METHOD="Builder.AndroidDev"
  OUTPUT_APK="mazerunner_dev.apk"
else
  BUILD_METHOD="Builder.AndroidRelease"
  OUTPUT_APK="mazerunner.apk"
fi

VERSION_SUFFIX="${UNITY_VERSION:0:4}"
RENAMED_APK="${OUTPUT_APK%.apk}_${VERSION_SUFFIX}.apk"

# === Build APK with Unity ===
echo "📦 Building APK ($BUILD_TYPE)..."
"$UNITY_PATH" $UNITY_CLI_ARGS -projectPath "$PROJECT_PATH" -executeMethod "$BUILD_METHOD"

# === Rename output APK ===
echo "📝 Renaming APK to $RENAMED_APK..."
mv "$PROJECT_PATH/$OUTPUT_APK" "$PROJECT_PATH/$RENAMED_APK"

echo "✅ Build completed: $RENAMED_APK"
