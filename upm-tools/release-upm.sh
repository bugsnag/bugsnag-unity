#!/bin/bash

GIT_DEPLOY=false
PACKAGE_FILE=../Bugsnag.unitypackage
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile unity.log"
PROJECT_PATH=`pwd`/UPMImportProject
SCRIPT_PATH=`pwd`


# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1

# check if we should deploy to github
if [  "$2" = "deploy" ] 
then
  echo "Will deploy to github"
  GIT_DEPLOY=true
else
    echo "Will not deploy to github, pass deploy as the second argument to deploy"
fi


#Build the plugin
echo "Building the sdk"

cd ..

#rake plugin:export

cd upm-tools


# make sure the package of the release is present
if [ ! -f "$PACKAGE_FILE" ]; then
    echo "$PACKAGE_FILE not found, please check for build errors."
    exit 1
fi

echo "SDK package found"

#check for the unity path
if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /Applications/Unity/Hub/Editor/2018.4.36f1/Unity.app/Contents/MacOS"
  exit 1
fi

echo "\`Unity\` executable = $UNITY_PATH/Unity"

#Import unitypackage into the Import project
echo "Importing Bugsnag.unitypackage into the import project"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $PROJECT_PATH \
                  -ignoreCompilerErrors \
                  -importPackage $SCRIPT_PATH/../Bugsnag.unitypackage


exit 0

# Shallow clone the UPM release branch
echo "Cloning the UPM release Branch"
git clone --depth=1 git@github.com:bugsnag/bugsnag-unity.git --branch upm-package PackageBranch

# Move the release files over to the package

echo "Copying over the unpacked sdk files"
cp -r UPMImportProject/Assets/Bugsnag/. PackageBranch/

echo "Copying over the package manifest"

cp package.json PackageBranch
cp package.json.meta PackageBranch

# Set the specified version in the manifest

echo "Setting the version $VERSION in the copied manifest"
sed -i '' "s/VERSION_STRING/$VERSION/g" "PackageBranch/package.json"

# Deploy to github with the release tag
if [ GIT_DEPLOY = true ] 
then
echo "Deploying to github with the tag upm-$VERSION"
cd PackageBranch
git add -A
git commit -m "release v$VERSION"
git tag "upm-$VERSION"
git push "upm-$VERSION"
cd ..
echo "cleaning up"

#clean out build files
git clean -fdx $SCRIPT_PATH
git rm PackageBranch
fi


echo "complete"
