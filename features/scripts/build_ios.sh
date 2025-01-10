#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_VERSION must be set"
  exit 1
fi

if [ -z "$1" ]
then
  echo "Build type must be specified: 'release' or 'dev'"
  exit 1
fi

BUILD_TYPE=$1

pushd "${0%/*}" >/dev/null
  script_path=$(pwd)
popd >/dev/null
pushd "$script_path/../fixtures" >/dev/null
  project_path="$(pwd)/maze_runner"
popd >/dev/null

# Clean any previous builds
find "$project_path/output/" -name "*.ipa" -exec rm '{}' \;

# Determine which Xcode project and IPA name to use
if [ "$BUILD_TYPE" == "dev" ]; then
  XCODE_PROJECT="mazerunner_dev_xcode/Unity-iPhone.xcodeproj"
  OUTPUT_IPA="mazerunner_dev_${UNITY_VERSION:0:4}.ipa"
elif [ "$BUILD_TYPE" == "release" ]; then
  XCODE_PROJECT="mazerunner_xcode/Unity-iPhone.xcodeproj"
  OUTPUT_IPA="mazerunner_${UNITY_VERSION:0:4}.ipa"
else
  echo "Invalid build type specified: 'release' or 'dev' only"
  exit 1
fi

# ------------------------------------------------------------------------------
# 1. ARCHIVE (equivalent to Product > Archive)
# ------------------------------------------------------------------------------
xcrun xcodebuild \
  -project "$project_path/$XCODE_PROJECT" \
  -scheme Unity-iPhone \
  -configuration Release \
  clean archive \
  -archivePath "$project_path/archive/Unity-iPhone.xcarchive" \
  -allowProvisioningUpdates \
  -allowProvisioningDeviceRegistration \
  -quiet \
  GCC_WARN_INHIBIT_ALL_WARNINGS=YES

if [ $? -ne 0 ]; then
  echo "Failed to archive project"
  exit 1
fi

# ------------------------------------------------------------------------------
# 2. EXPORT ARCHIVE
# ------------------------------------------------------------------------------
xcrun xcodebuild -exportArchive \
  -archivePath "$project_path/archive/Unity-iPhone.xcarchive" \
  -exportPath "$project_path/output/" \
  -exportOptionsPlist "$script_path/exportOptions.plist" \
  -quiet

if [ $? -ne 0 ]; then
  echo "Failed to export app"
  exit 1
fi

# ------------------------------------------------------------------------------
# 3. MOVE IPA TO A KNOWN LOCATION
# ------------------------------------------------------------------------------
find "$project_path/output/" -name "*.ipa" -exec mv '{}' "$project_path/$OUTPUT_IPA" \;

echo "Successfully built and exported: $OUTPUT_IPA"