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

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -logFile unity.log"

if [[ "$UNITY_PATH" == *"2020"* ]]; then
  # Copy the 2020 package manifest in for that version
  cp manifests/manifest_2020.json maze_runner/Packages/manifest.json
elif [[ "$UNITY_PATH" == *"2019" ]]; then
  # Copy the 2019 package manifest in for that version
  cp manifests/manifest_2019.json maze_runner/Packages/manifest.json
else
  # On all other versions disable the package manager
  DEFAULT_CLI_ARGS="${DEFAULT_CLI_ARGS} -noUpm"
fi

project_path=`pwd`/maze_runner

# Build for Android
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.AndroidBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/mazerunner.apk $project_path/mazerunner_$UNITY_VERSION.apk
