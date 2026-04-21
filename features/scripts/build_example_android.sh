#!/bin/bash
set -e

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
log_file="$builder_path/build_android_example.log"
OUTPUT_APK="example.apk"
RENAMED_APK="example_${UNITY_VERSION:0:4}.apk"

echo "Building Android example app with Unity $UNITY_VERSION"

# Import Bugsnag package FIRST (example scripts depend on it)
echo "Importing Bugsnag.unitypackage into builder project"
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -importPackage "$REPO_ROOT/Bugsnag.unitypackage"

# Then copy example app assets (which reference Bugsnag types)
echo "Copying example assets to builder project..."
rm -rf "$builder_path/Assets/Scenes" "$builder_path/Assets/Scripts"
cp -R "$example_source/Assets/Scenes" "$builder_path/Assets/"
cp -R "$example_source/Assets/Scripts" "$builder_path/Assets/"
cp -R "$example_source/ProjectSettings" "$builder_path/"

# Build Android
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -executeMethod ExampleAppBuilder.AndroidRelease

RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Android example build failed"
  exit $RESULT
fi

# Unity 2021+ with IL2CPP/Gradle leaves the APK in the Gradle build directory
if [ ! -f "$builder_path/$OUTPUT_APK" ]; then
  GRADLE_APK="$builder_path/Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/apk/release/launcher-release.apk"
  if [ -f "$GRADLE_APK" ]; then
    echo "APK found in Gradle output directory, copying to project root..."
    cp "$GRADLE_APK" "$builder_path/$OUTPUT_APK"
  else
    echo "ERROR: APK not found at $builder_path/$OUTPUT_APK or $GRADLE_APK"
    exit 1
  fi
fi

# Move to example directory for artifact upload
mv "$builder_path/$OUTPUT_APK" "$example_source/$RENAMED_APK"

echo "Android example app built successfully: $example_source/$RENAMED_APK"
