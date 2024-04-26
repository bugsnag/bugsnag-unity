#!/usr/bin/env bash

pushd "${0%/*}"
  script_path=`pwd`
popd
pushd "$script_path/../fixtures"
project_path=`pwd`/maze_runner

# Clean any previous builds
find $project_path/output/ -name "*.ipa" -exec rm '{}' \;

# Archive and export the project
xcrun xcodebuild -project $project_path/mazerunner_xcode/Unity-iPhone.xcodeproj \
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
find $project_path/output/ -name "*.ipa" -exec mv '{}' $project_path/mazerunner_${UNITY_VERSION:0:4}.ipa \;
