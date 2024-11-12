#!/bin/bash

PACKAGE_DIR=upm-package

# Check for release version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1

echo "Copying over the unpacked sdk files"
cp -r "Bugsnag/Assets/Bugsnag/." $PACKAGE_DIR

echo "Copying over the package manifest and readme"

cp upm/package.json $PACKAGE_DIR
cp upm/package.json.meta $PACKAGE_DIR
cp upm/README.md $PACKAGE_DIR
cp upm/README.md.meta $PACKAGE_DIR

# remove EDM menu from package 
rm "$PACKAGE_DIR/Editor/BugsnagEditor.EDM.cs"
rm "$PACKAGE_DIR/Editor/BugsnagEditor.EDM.cs.meta"

# Set the specified version in the manifest

echo "Setting the version $VERSION in the copied manifest and readme"
sed -i '' "s/VERSION_STRING/$VERSION/g" "$PACKAGE_DIR/package.json"
sed -i '' "s/VERSION_STRING/v$VERSION/g" "$PACKAGE_DIR/README.md"


echo "complete, ready to deploy"