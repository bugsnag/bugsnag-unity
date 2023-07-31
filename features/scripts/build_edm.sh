#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
project_path=`pwd`/EDM_Fixture

# Installing the Bugsnag package
echo "Importing Bugsnag.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -logFile $script_path/edmImport.log\
                  -importPackage $script_path/../../Bugsnag.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "Enable EDM"


$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -buildTarget Android -logFile $script_path/enableEdm.log -projectPath $project_path -executeMethod Builder.EnableEDM
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "Build EDM APK"


$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -buildTarget Android -logFile $script_path/buildEdmFixture.log -projectPath $project_path -executeMethod Builder.AndroidBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

  mv $project_path/edm.apk $project_path/edm_$UNITY_VERSION.apk
