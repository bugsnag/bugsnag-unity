#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

DEFAULT_CLI_ARGS="-quit -batchmode -nographics"

project_path=features/fixtures/minimalapp

package_path=Bugsnag.unitypackage

imported_sdk_path="$project_path/Assets/Bugsnag"

rm -rf "$project_path/Assets/Bugsnag"

echo "building android without bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildAndroidWithout -logFile build_android_minimal_without.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "building ios without bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildIosWithout -logFile export_ios_xcode_project_minimal_without.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

source ./features/scripts/build_xcode_project.sh features/fixtures/minimalapp/minimal_without_xcode without_bugsnag

ls

mv -f $package_path $project_path

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -importPackage $package_path
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi


echo "building android with bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildAndroidWith -logFile build_android_minimal_with.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "building ios with bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildIosWith -logFile export_ios_xcode_project_minimal_with.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
ls

source ./features/scripts/build_xcode_project.sh features/fixtures/minimalapp/minimal_with_xcode with_bugsnag

cd features/fixtures/minimalapp

bundle install
bundle exec danger