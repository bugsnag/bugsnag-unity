#!/usr/bin/env bash

if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /Applications/Unity/Hub/Editor/2018.4.36f1/Unity.app/Contents/MacOS"
  exit 1
fi

echo "\`Unity\` executable = $UNITY_PATH/Unity"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../../fixtures"

DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
project_path=`pwd`/maze_runner

# Installing the Bugsnag package
echo "Importing external-dependency-manager-1.2.169.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -logFile edmImport.log\
                  -importPackage $script_path/../../EDM4U/external-dependency-manager-1.2.169.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -logFile edmEnable.log -projectPath $project_path -executeMethod Builder.EnableEDM
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
