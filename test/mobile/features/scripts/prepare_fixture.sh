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

# Installing the Bugsnag package
echo "Importing Bugsnag.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -importPackage $script_path/../../../../Bugsnag.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "Importing Bugsnag-with-android-64bit.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -importPackage $script_path/../../../../Bugsnag-with-android-64bit.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
