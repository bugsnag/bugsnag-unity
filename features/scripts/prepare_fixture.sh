#!/usr/bin/env bash
set -euo pipefail

# === Check environment ===
if [[ -z "${UNITY_VERSION:-}" ]]; then
  echo "❌ UNITY_VERSION must be set"
  exit 1
fi

# === Constants ===
UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
echo "script_dir: $SCRIPT_DIR"
FIXTURE_DIR="$SCRIPT_DIR/../fixtures"
PROJECT_PATH="$FIXTURE_DIR/maze_runner"
BUGSNAG_PACKAGE="$SCRIPT_DIR/../../Bugsnag.unitypackage"
PERF_REPO_URL="https://github.com/bugsnag/bugsnag-unity-performance-upm.git"
PERF_PACKAGE_PATH="$PROJECT_PATH/packages/performance-package"
AAR_OUTPUT="$PROJECT_PATH/Assets/Plugins/Android/mazerunner_code.aar"

DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile unity.log"

cd "$FIXTURE_DIR"

if [ "$1" == "android" ]; then
  echo "🛠️  Building Android mazerunner AAR..."
  ./maze_runner/nativeplugin/android/gradlew -p maze_runner/nativeplugin/android assembleRelease
  if [ $? -ne 0 ]; then
    echo "❌ Failed to build Android AAR"
    exit 1
  fi

  echo "📦 Copying AAR to Unity plugins directory..."
  cp maze_runner/nativeplugin/android/build/outputs/aar/android-release.aar "$AAR_OUTPUT"
  if [ $? -ne 0 ]; then
    echo "❌ Failed to copy AAR to Unity plugins directory"
    exit 1
  fi
fi

# === Import Bugsnag package ===
echo "📥 Importing Bugsnag.unitypackage into Unity project..."
"$UNITY_PATH" $DEFAULT_CLI_ARGS \
  -projectPath "$PROJECT_PATH" \
  -ignoreCompilerErrors \
  -importPackage "$BUGSNAG_PACKAGE"

# === Import performance UPM package ===
echo "🧬 Cloning bugsnag-unity-performance-upm..."
rm -rf "$PERF_PACKAGE_PATH"
git clone --depth=1 "$PERF_REPO_URL" "$PERF_PACKAGE_PATH"

echo "✅ Build completed successfully!"
