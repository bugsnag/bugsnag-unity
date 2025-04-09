#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

# Run Unity and immediately exit afterwards, logging all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
PROJECT_PATH="Bugsnag"

# Generate dotnet sln and proj files
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath "$PROJECT_PATH" -executeMethod "UnityEditor.SyncVS.SyncSolution"
RESULT=$?

if [ $RESULT -ne 0 ]; then
  exit $RESULT
fi

# Decide which dotnet format command to run based on the argument
FORMAT_COMMAND="dotnet format"
if [ "$1" == "--verify" ]; then
  FORMAT_COMMAND="dotnet format --verify-no-changes"
fi

# Run the selected dotnet format command
$FORMAT_COMMAND "$PROJECT_PATH/Bugsnag.sln"