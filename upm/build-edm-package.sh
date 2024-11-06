#!/bin/bash

# This script takes the packaged produced by the build-upm-package.sh script and 
# converts it to support EDM4U so that we can release a UPM version with EDM4U support.

PACKAGE_DIR=../upm-package

# Remove bundled kotlin libs
rm -r "$PACKAGE_DIR/Plugins/Android/Kotlin"
rm "$PACKAGE_DIR/Plugins/Android/Kotlin.meta"

# Copy in the EDM4U manifest
cp EDM/BugsnagAndroidDependencies.xml "$PACKAGE_DIR/Editor"
cp EDM/BugsnagAndroidDependencies.xml.meta "$PACKAGE_DIR/Editor"

# Change the readme title to reference EDM4U
sed -i '' "s/Bugsnag SDK for Unity/Bugsnag SDK for Unity Including EDM4U Support/g" "$PACKAGE_DIR/README.md"
sed -i '' "s/bugsnag-unity-upm.git/bugsnag-unity-upm-edm4u.git/g" "$PACKAGE_DIR/README.md"