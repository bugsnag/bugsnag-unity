#!/usr/bin/env bash
set -euo pipefail

# === Validate Environment ===
if [[ -z "${UNITY_VERSION:-}" ]]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

# === Constants & Paths ===
UNITY_PATH="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
FIXTURE_PATH="features/fixtures/EDM_Fixture"
OUTPUT_APK="edm4u.apk"
RENAMED_APK="edm4u_${UNITY_VERSION:0:4}.apk"
ROOT_PATH="$(pwd)"
DESTINATION="$FIXTURE_PATH/Packages"
PACKAGE_ZIP="$ROOT_PATH/upm-edm4u-package.zip"
LOG_FILE="build-edm4u.log"
DEFAULT_CLI_ARGS="-batchmode -nographics -quit -ignoreCompilerErrors -logFile $LOG_FILE"

# === Unzip UPM Package ===
echo "📦 Unzipping EDM4U package into $DESTINATION..."
rm -rf "$DESTINATION/package"
mkdir -p "$DESTINATION"
unzip -q "$PACKAGE_ZIP" -d "$DESTINATION"

# === Clean macOS metadata ===
MACOSX_DIR="$DESTINATION/__MACOSX"
if [[ -d "$MACOSX_DIR" ]]; then
  echo "🧹 Removing macOS metadata folder..."
  rm -rf "$MACOSX_DIR"
fi

echo "✅ Package unzipped successfully"

# === Build Android APK with Unity ===
echo "🚀 Building Android APK..."
"$UNITY_PATH" $DEFAULT_CLI_ARGS \
  -projectPath "$FIXTURE_PATH" \
  -executeMethod Builder.AndroidBuild

# === Rename APK with version suffix ===
APK_PATH="$FIXTURE_PATH/$OUTPUT_APK"
if [[ ! -f "$APK_PATH" ]]; then
  echo "❌ Expected APK not found at: $APK_PATH"
  exit 2
fi

echo "📝 Renaming APK to $RENAMED_APK..."
mv -f "$APK_PATH" "$FIXTURE_PATH/$RENAMED_APK"

echo "🎉 Build completed: $FIXTURE_PATH/$RENAMED_APK"
