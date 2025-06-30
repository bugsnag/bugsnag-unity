#!/usr/bin/env bash
set -euo pipefail

# === Validate Unity version ===
if [ -z "${UNITY_VERSION:-}" ]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"
FIXTURE_PATH="features/fixtures/maze_runner"
DEFAULT_CLI_ARGS="-batchmode -nographics -quit"
PACKAGE_ZIP="upm-package.zip"
DESTINATION="$FIXTURE_PATH/Packages"
UNZIP_LOG="upm-import.log"

# === Unzip the UPM package ===
echo "📦 Unzipping $PACKAGE_ZIP to $DESTINATION/package..."
rm -rf "$DESTINATION/package"
mkdir -p "$DESTINATION"
unzip -q "$PACKAGE_ZIP" -d "$DESTINATION"

# === Clean up macOS metadata folder ===
MACOSX_FOLDER="$DESTINATION/__MACOSX"
if [ -d "$MACOSX_FOLDER" ]; then
  echo "🧹 Removing macOS metadata folder: $MACOSX_FOLDER"
  rm -rf "$MACOSX_FOLDER"
fi

echo "✅ Package unzipped successfully"

# === Trigger Unity script compilation ===
echo "🧠 Triggering Unity script compilation..."
"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
  -projectPath "$FIXTURE_PATH" \
  -logFile "$UNZIP_LOG" \
  -executeMethod UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation

echo "✅ Unity script compilation completed"
