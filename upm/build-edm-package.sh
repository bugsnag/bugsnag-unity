#!/bin/bash

# This script takes the package produced by the build-upm-package.sh script and 
# converts it to support EDM4U so that we can release a UPM version with EDM4U support.

PACKAGE_DIR=upm-package
EDM_PACKAGE_DIR=upm-edm4u-package

# Create a copy of the package directory
cp -r "$PACKAGE_DIR" "$EDM_PACKAGE_DIR"

# Remove bundled Kotlin libs from the copied package
test -d "$EDM_PACKAGE_DIR/Plugins/Android/Kotlin" && rm -r "$EDM_PACKAGE_DIR/Plugins/Android/Kotlin"
test -f "$EDM_PACKAGE_DIR/Plugins/Android/Kotlin.meta" && rm "$EDM_PACKAGE_DIR/Plugins/Android/Kotlin.meta"

# Copy in the EDM4U manifest
cp upm/EDM/BugsnagAndroidDependencies.xml "$EDM_PACKAGE_DIR/Editor"
cp upm/EDM/BugsnagAndroidDependencies.xml.meta "$EDM_PACKAGE_DIR/Editor"

# Change the readme title to reference EDM4U
sed -i "" "s/Bugsnag SDK for Unity/Bugsnag SDK for Unity Including EDM4U Support/g" "$EDM_PACKAGE_DIR/README.md"
sed -i "" "s/bugsnag-unity-upm.git/bugsnag-unity-upm-edm4u.git/g" "$EDM_PACKAGE_DIR/README.md"

echo "EDM4U-supported package created at: $EDM_PACKAGE_DIR"
