#!/bin/bash
set -Eeuo pipefail

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION"
if [ ! -d "$UNITY_PATH" ]; then
  echo "Unity $UNITY_VERSION not found at $UNITY_PATH"
  exit 1
fi

# CI-specific builder project
REPO_ROOT="$(pwd)"
builder_path="$REPO_ROOT/features/fixtures/example_builder"
example_source="$REPO_ROOT/example"

# Write the Unity log into the example folder so Buildkite artifact upload can always find it
log_file="$example_source/build_ios_example.log"
XCODE_PROJECT="example_xcode"
IPA_OUTPUT="example_${UNITY_VERSION:0:4}.ipa"

echo "Building iOS example app with Unity $UNITY_VERSION"

# Always try to preserve the Unity log for CI debugging
mkdir -p "$example_source"
rm -f "$log_file" 2>/dev/null || true
touch "$log_file"
trap 'cp -f "$log_file" "$example_source/build_ios_example.log" 2>/dev/null || true' EXIT

# Unity needs ProjectSettings to know about scenes and build settings
# Copy minimal required ProjectSettings files only
echo "Copying required ProjectSettings (EditorBuildSettings, InputManager, GraphicsSettings)..."
mkdir -p "$builder_path/ProjectSettings"
cp "$example_source/ProjectSettings/EditorBuildSettings.asset" "$builder_path/ProjectSettings/" 2>/dev/null || true
cp "$example_source/ProjectSettings/InputManager.asset" "$builder_path/ProjectSettings/" 2>/dev/null || true
cp "$example_source/ProjectSettings/GraphicsSettings.asset" "$builder_path/ProjectSettings/" 2>/dev/null || true

# Import Bugsnag package (example scripts depend on it)
echo "Importing Bugsnag.unitypackage into builder project"
set +e
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -importPackage "$REPO_ROOT/Bugsnag.unitypackage"
IMPORT_RESULT=$?
set -e

if [ $IMPORT_RESULT -ne 0 ]; then
  echo "Unity import failed with exit code $IMPORT_RESULT"
  echo "=== Unity log tail (import) ==="
  tail -n 200 "$log_file" || true
  echo "=== Compiler errors (import) ==="
  grep -E "error CS[0-9]+|Scripts have compiler errors" -n "$log_file" || true
  exit $IMPORT_RESULT
fi

# Then copy example app assets (which reference Bugsnag types)
echo "Copying example assets to builder project..."
rm -rf "$builder_path/Assets/Scenes" "$builder_path/Assets/Scripts" "$builder_path/Assets/Resources" "$builder_path/Assets/Plugins" 2>/dev/null || true
mkdir -p "$builder_path/Assets"
cp -R "$example_source/Assets/Scenes" "$builder_path/Assets/" 2>/dev/null || true
cp -R "$example_source/Assets/Scripts" "$builder_path/Assets/" 2>/dev/null || true
cp -R "$example_source/Assets/Resources" "$builder_path/Assets/" 2>/dev/null || true
cp -R "$example_source/Assets/Plugins" "$builder_path/Assets/" 2>/dev/null || true

# Build iOS Xcode project
echo "Building iOS Xcode project..."
set +e
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -executeMethod ExampleAppBuilder.IosRelease
BUILD_RESULT=$?
set -e

if [ $BUILD_RESULT -ne 0 ]; then
  echo "iOS example build failed with exit code $BUILD_RESULT"
  echo "=== Unity log tail (build) ==="
  tail -n 200 "$log_file" || true
  echo "=== Compiler errors (build) ==="
  grep -E "error CS[0-9]+|Scripts have compiler errors" -n "$log_file" || true
  exit $BUILD_RESULT
fi

XCODE_PATH="$builder_path/$XCODE_PROJECT"

if [ ! -d "$XCODE_PATH" ]; then
  echo "ERROR: Xcode project not found at $XCODE_PATH"
  tail -n 100 "$log_file" || true
  exit 1
fi

# Build and archive the Xcode project
echo "Building Xcode project with xcodebuild..."
pushd "$XCODE_PATH"

ARCHIVE_PATH="$PWD/example.xcarchive"

xcodebuild \
  -project Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Release \
  -archivePath "$ARCHIVE_PATH" \
  archive \
  CODE_SIGN_IDENTITY="" \
  CODE_SIGNING_REQUIRED=NO \
  CODE_SIGNING_ALLOWED=NO

if [ $? -ne 0 ]; then
  echo "Xcode archive build failed"
  exit 1
fi

# Package as unsigned IPA (CI doesn't have signing certificates)
echo "Creating unsigned IPA from archive..."
mkdir -p "$ARCHIVE_PATH/Products/Applications"
cp -R "$ARCHIVE_PATH/Products/Applications/Unity-iPhone.app" "$PWD/" 2>/dev/null || true
cp -R "$ARCHIVE_PATH/Products/Applications/"*.app "$PWD/" 2>/dev/null || true

# Find the .app bundle
APP_BUNDLE=$(find "$PWD" -maxdepth 1 -name "*.app" | head -n 1)
if [ -z "$APP_BUNDLE" ]; then
  echo "ERROR: Could not find .app bundle"
  exit 1
fi

# Create Payload directory and IPA
rm -rf Payload 2>/dev/null || true
mkdir -p Payload
cp -R "$APP_BUNDLE" Payload/
zip -qr "$example_source/$IPA_OUTPUT" Payload

popd

if [ -f "$example_source/$IPA_OUTPUT" ]; then
  echo "iOS example app built successfully: $example_source/$IPA_OUTPUT"
else
  echo "ERROR: IPA not created"
  exit 1
fi
