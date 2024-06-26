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

pushd "${0%/*}"
  script_path=`pwd`
popd
pushd "$script_path/../fixtures"
project_path=`pwd`/maze_runner

# Clean any previous builds
find $project_path/output/ -name "*.ipa" -exec rm '{}' \;

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

# Archive and export the project
xcrun xcodebuild -project $project_path/$XCODE_PROJECT \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath $project_path/archive/Unity-iPhone.xcarchive \
                 -allowProvisioningUpdates \
                 -allowProvisioningDeviceRegistration \
                 -quiet \
                 GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
                 archive

if [ $? -ne 0 ]
then
  echo "Failed to archive project"
  exit 1
fi

xcrun xcodebuild -exportArchive \
                 -archivePath $project_path/archive/Unity-iPhone.xcarchive \
                 -exportPath $project_path/output/ \
                 -quiet \
                 -exportOptionsPlist $script_path/exportOptions.plist

if [ $? -ne 0 ]; then
  echo "Failed to export app"
  exit 1
fi

# Move to known location for running (note - the name of the .ipa differs between Xcode versions)
find $project_path/output/ -name "*.ipa" -exec mv '{}' $project_path/$OUTPUT_IPA \;
