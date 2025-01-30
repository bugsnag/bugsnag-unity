#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"


FIXTURE_PATH="features/fixtures/maze_runner"
DEFAULT_CLI_ARGS="-batchmode -nographics -quit"

# Proceed with unzipping the main package
root_path=$(pwd)
destination="features/fixtures/maze_runner/Packages"
package="$root_path/upm-package.zip"

rm -rf "$destination/package"
unzip -q "$package" -d "$destination"

# Remove the __MACOSX directory if it exists
if [ -d "$destination/__MACOSX" ]; then
  rm -rf "$destination/__MACOSX"
fi

echo "Package unzipped successfully"

$UNITY_PATH/Unity -batchmode -quit -projectPath $FIXTURE_PATH -logFile upm-import.log -executeMethod UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation