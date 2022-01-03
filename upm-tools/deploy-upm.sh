#!/bin/bash

PACKAGE_DIR=../upm-package

# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1

echo "Deploying to github with the tag upm-$VERSION"
#git add $PACKAGE_DIR
#git commit -m "upm release v$VERSION"
#git push
git tag "upm-$VERSION"
git push origin "upm-$VERSION"
