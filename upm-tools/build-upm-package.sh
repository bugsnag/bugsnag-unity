#!/bin/bash

GIT_DEPLOY=false
PACKAGE_FILE=../Bugsnag.unitypackage
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile unity.log"
PROJECT_PATH=`pwd`/UPMImportProject
SCRIPT_PATH=`pwd`
PACKAGE_DIR=../upm-package

# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1

#check for the unity path
if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /Applications/Unity/Hub/Editor/2018.4.36f1/Unity.app/Contents/MacOS"
  exit 1
fi

#Build the plugin
echo "Building the sdk"

cd ..

rake plugin:export

cd upm-tools


# make sure the package of the release is present after building
if [ ! -f "$PACKAGE_FILE" ]; then
    echo "$PACKAGE_FILE not found, please check for build errors."
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


# Set the specified version in the manifest

echo "Setting the version $VERSION in the copied manifest and readme"
sed -i '' "s/VERSION_STRING/$VERSION/g" "$PACKAGE_DIR/package.json"
sed -i '' "s/VERSION_STRING/v$VERSION/g" "$PACKAGE_DIR/README.md"


echo "complete, ready to deploy"
