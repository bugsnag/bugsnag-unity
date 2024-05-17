#!/bin/bash

GIT_DEPLOY=false
PACKAGE_FILE=../Bugsnag.unitypackage
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile unity.log"
PROJECT_PATH=`pwd`/UPMImportProject
SCRIPT_PATH=`pwd`
PACKAGE_DIR=../upm-package

# Check for release version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1

if [ -z "$UNITY_UPM_VERSION" ]
then
  echo "UNITY_UPM_VERSION must be set"
  exit 1
fi

#There is a bug in some versions of unity 2020, 2021 and 2022 where macos bundles will not be imported as a single plugin file.
#In which case all sub dirs and files must have .meta files to work with UPM.
#Building the UPM package with unity 2019 ensures that the meta files are created 

if [[ "$UNITY_UPM_VERSION" != *"2019"* ]]; then
  echo "ERROR: UNITY_UPM_VERSION must be a version of Unity 2019. See script comments for details."
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_UPM_VERSION/Unity.app/Contents/MacOS"



#Check for the release package
echo "Checking for the release package"

# make sure the package of the release is present after building
if [ ! -f "$PACKAGE_FILE" ]; then
    echo "$PACKAGE_FILE not found, please provide a release package."
    exit 1
fi

echo "SDK package found"

echo "\`Unity\` executable = $UNITY_PATH/Unity"

#Import unitypackage into the Import project
echo "Importing Bugsnag.unitypackage into the import project"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $PROJECT_PATH \
                  -ignoreCompilerErrors \
                  -importPackage $SCRIPT_PATH/../Bugsnag.unitypackage

echo "Copying over the unpacked sdk files"
cp -r UPMImportProject/Assets/Bugsnag/. $PACKAGE_DIR

echo "Copying over the package manifest, asembly defs and readme"

cp package.json $PACKAGE_DIR
cp package.json.meta $PACKAGE_DIR
cp README.md $PACKAGE_DIR
cp README.md.meta $PACKAGE_DIR
cp AssemblyDefinitions/Bugsnag.asmdef "$PACKAGE_DIR/Scripts"
cp AssemblyDefinitions/Bugsnag.asmdef.meta "$PACKAGE_DIR/Scripts"
cp AssemblyDefinitions/BugsnagEditor.asmdef "$PACKAGE_DIR/Editor"
cp AssemblyDefinitions/BugsnagEditor.asmdef.meta "$PACKAGE_DIR/Editor"


# remove EDM menu from package 
rm "$PACKAGE_DIR/Editor/BugsnagEditor.EDM.cs"
rm "$PACKAGE_DIR/Editor/BugsnagEditor.EDM.cs.meta"

# Set the specified version in the manifest

echo "Setting the version $VERSION in the copied manifest and readme"
sed -i '' "s/VERSION_STRING/$VERSION/g" "$PACKAGE_DIR/package.json"
sed -i '' "s/VERSION_STRING/v$VERSION/g" "$PACKAGE_DIR/README.md"


echo "complete, ready to deploy"
