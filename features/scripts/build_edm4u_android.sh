#!/usr/bin/env bash

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"
OUTPUT_APK="edm4u.apk"
RENAMED_APK="edm4u_${UNITY_VERSION:0:4}.apk"

FIXTURE_PATH="features/fixtures/EDM_Fixture"
DEFAULT_CLI_ARGS="-batchmode -nographics -quit"

# Proceed with unzipping the main package
root_path=$(pwd)
destination="features/fixtures/EDM_Fixture/Packages"
package="$root_path/upm-edm4u-package.zip"

rm -rf "$destination/package"
unzip -q "$package" -d "$destination"

# Remove the __MACOSX directory if it exists
if [ -d "$destination/__MACOSX" ]; then
  rm -rf "$destination/__MACOSX"
fi

echo "Package unzipped successfully"

$UNITY_PATH/Unity -batchmode -quit -nographics -ignoreCompilerErrors -projectPath $FIXTURE_PATH -logFile build-edm4u.log -executeMethod Builder.AndroidBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi


mv $FIXTURE_PATH/$OUTPUT_APK $FIXTURE_PATH/$RENAMED_APK

  