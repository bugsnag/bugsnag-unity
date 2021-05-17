#!/usr/bin/env bash

pushd "${0%/*}"
  script_path=`pwd`
popd
pushd "$script_path/../fixtures"
project_path=`pwd`/maze_runner

# Archive and export the project
xcrun xcodebuild -project $project_path/mazerunner_xcode/Unity-iPhone.xcodeproj \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath $project_path/archive/Unity-iPhone.xcarchive \
                 -allowProvisioningUpdates \
                 -allowProvisioningDeviceRegistration \
                 -quiet \
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

mv $project_path/output/Unity-iPhone.ipa $project_path/mazerunner_$UNITY_VERSION.ipa
