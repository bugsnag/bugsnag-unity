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
builder_path="$(pwd)/features/fixtures/example_builder"
example_source="$(pwd)/example"
log_file="$builder_path/build_ios_example.log"
XCODE_PROJECT="example_xcode"
IPA_OUTPUT="example_${UNITY_VERSION:0:4}.ipa"

echo "Building iOS example app with Unity $UNITY_VERSION"

# Import Bugsnag package FIRST (example scripts depend on it)
echo "Importing Bugsnag.unitypackage into builder project"
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -importPackage "$(pwd)/Bugsnag.unitypackage"

# Then copy example app assets (which reference Bugsnag types)
echo "Copying example assets to builder project..."
rm -rf "$builder_path/Assets/Scenes" "$builder_path/Assets/Scripts"
cp -R "$example_source/Assets/Scenes" "$builder_path/Assets/"
cp -R "$example_source/Assets/Scripts" "$builder_path/Assets/"
cp -R "$example_source/ProjectSettings" "$builder_path/"

# Build iOS Xcode project
$UNITY_PATH/Unity.app/Contents/MacOS/Unity \
  -nographics \
  -quit \
  -batchmode \
  -logFile "$log_file" \
  -projectPath "$builder_path" \
  -executeMethod ExampleAppBuilder.IosRelease

RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "iOS example build failed"
  exit $RESULT
fi

# Build and archive Xcode project
cd "$builder_path/$XCODE_PROJECT"

xcodebuild \
  -project Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Release \
  -archivePath "$builder_path/archive/Unity-iPhone.xcarchive" \
  archive \
  CODE_SIGN_IDENTITY="" \
  CODE_SIGNING_REQUIRED=NO \
  CODE_SIGNING_ALLOWED=NO

# Export IPA
xcodebuild \
  -exportArchive \
  -archivePath "$builder_path/archive/Unity-iPhone.xcarchive" \
  -exportPath "$builder_path" \
  -exportOptionsPlist "$(pwd)/features/scripts/exportOptions.plist"

# Move to example directory for artifact upload
if [ -f "$builder_path/Unity-iPhone.ipa" ]; then
  mv "$builder_path/Unity-iPhone.ipa" "$example_source/$IPA_OUTPUT"
  echo "iOS example app built successfully: $example_source/$IPA_OUTPUT"
else
  echo "ERROR: IPA not found at $builder_path/Unity-iPhone.ipa"
  exit 1
fi
