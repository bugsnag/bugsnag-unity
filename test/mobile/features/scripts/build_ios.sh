#!/usr/bin/env bash

if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /Applications/Unity/Hub/Editor/2017.4.40f1/Unity.app/Contents/MacOS"
  exit 1
fi

echo "\`which Unity\`=`which Unity`"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output, disable the
# package manager (we just don't need it and it slows things down)
DEFAULT_CLI_ARGS="-quit -batchmode -logFile unity.log -noUpm"
project_path=`pwd`/maze_runner

# Generate the Xcode project for iOS
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.IosBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

# Archive and export the project
xcrun xcodebuild -project $project_path/mazerunner_xcode/Unity-iPhone.xcodeproj \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath $project_path/archive/Unity-iPhone.xcarchive \
                 -allowProvisioningUpdates \
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
