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
log_file="$example_source/build_android_example.log"
OUTPUT_APK="example.apk"
RENAMED_APK="example_${UNITY_VERSION:0:4}.apk"

echo "Building Android example app with Unity $UNITY_VERSION"

# Always try to preserve the Unity log for CI debugging
mkdir -p "$example_source"
rm -f "$log_file" 2>/dev/null || true
touch "$log_file"
trap 'cp -f "$log_file" "$example_source/build_android_example.log" 2>/dev/null || true' EXIT

# Ensure clean Android build environment
mkdir -p "$HOME/.android"
rm -rf "$builder_path/.gradle-user-home" 2>/dev/null || true
mkdir -p "$builder_path/.gradle-user-home"
export GRADLE_USER_HOME="$builder_path/.gradle-user-home"

# Unity needs ProjectSettings to know about scenes and build settings
# Copy minimal required ProjectSettings files only
echo "Copying required ProjectSettings (EditorBuildSettings, InputManager, GraphicsSettings)..."
mkdir -p "$builder_path/ProjectSettings"
cp "$example_source/ProjectSettings/EditorBuildSettings.asset" "$builder_path/ProjectSettings/" 2>/dev/null || true
cp "$example_source/ProjectSettings/InputManager.asset" "$builder_path/ProjectSettings/" 2>/dev/null || true
cp "$example_source/ProjectSettings/GraphicsSettings.asset" "$builder_path/ProjectSettings/" 2>/dev/null || true

# Ensure the builder project directory exists and print contents for CI debugging
mkdir -p "$builder_path"
echo "Builder project path: $builder_path"
echo "Contents of builder project (top-level):"
ls -la "$builder_path" || true

# Copy Packages/ to ensure Unity Package Manager manifest is present (eg. com.unity.ugui)
echo "Copying Packages/manifest.json if present (skipping for Unity 2021 to preserve behavior)..."
if [[ "$UNITY_VERSION" == 2021.* ]]; then
  echo "Unity 2021 detected; skipping Packages copy to preserve 2021 behavior"
else
  if [ -d "$example_source/Packages" ] || [ -f "$example_source/Packages/manifest.json" ]; then
    mkdir -p "$builder_path/Packages"
    cp -R "$example_source/Packages/" "$builder_path/Packages/" 2>/dev/null || true
    echo "Packages copied:" && ls -la "$builder_path/Packages" || true
  else
    echo "No Packages/ found in example source"
  fi
fi

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

# Build Android
echo "Building Android APK..."
set +e
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -executeMethod ExampleAppBuilder.AndroidRelease
RESULT=$?
set -e

if [ $RESULT -ne 0 ]; then 
  echo "Android example build failed with exit code $RESULT"
  echo "=== Build log tail ==="
  tail -n 200 "$log_file" || true
  echo "=== Compiler errors ==="
  grep -E "error CS[0-9]+|Scripts have compiler errors|CommandInvokationFailure|Gradle build failed" -n "$log_file" || true
  exit $RESULT
fi

# Unity 2021+ with IL2CPP/Gradle leaves the APK in the Gradle build directory
if [ ! -f "$builder_path/$OUTPUT_APK" ]; then
  # Unity places APKs in different locations depending on version and build pipeline.
  CANDIDATES=(
    "$builder_path/Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/apk/release/launcher-release.apk"
    "$builder_path/Temp/gradleOut/launcher/build/outputs/apk/release/launcher-release.apk"
  )

  FOUND_APK=""
  for candidate in "${CANDIDATES[@]}"; do
    if [ -f "$candidate" ]; then
      FOUND_APK="$candidate"
      break
    fi
  done

  if [ -n "$FOUND_APK" ]; then
    echo "APK found at $FOUND_APK, copying to project root..."
    cp "$FOUND_APK" "$builder_path/$OUTPUT_APK"
  else
    echo "ERROR: APK not found at $builder_path/$OUTPUT_APK and no Gradle output APK was located"
    echo "Searched locations:"
    for candidate in "${CANDIDATES[@]}"; do
      echo "  - $candidate"
    done
    echo "=== Unity log tail ==="
    tail -n 200 "$log_file" || true
    exit 1
  fi
fi

# Move to example directory for artifact upload
mv "$builder_path/$OUTPUT_APK" "$example_source/$RENAMED_APK"

echo "Android example app built successfully: $example_source/$RENAMED_APK"
