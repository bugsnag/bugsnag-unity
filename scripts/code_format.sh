#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"

PROJECT_PATH="Bugsnag"

# generate dotnet sln and proj files
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $PROJECT_PATH -executeMethod "UnityEditor.SyncVS.SyncSolution"
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

dotnet format "Bugsnag/Bugsnag.sln"