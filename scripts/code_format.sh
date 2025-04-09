#!/usr/bin/env bash
set -e  

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
PROJECT_PATH="Bugsnag"

# Generate .sln and project files
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

# Execute dotnet format
$FORMAT_COMMAND "$PROJECT_PATH/Bugsnag.sln"
EXIT_CODE=$?

if [ "$EXIT_CODE" -ne 0 ]; then
  echo "Error: Code formatting verification found issues."
  exit "$EXIT_CODE"
fi

echo "Done."