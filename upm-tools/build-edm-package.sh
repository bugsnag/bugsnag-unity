#!/bin/bash
PACKAGE_DIR=../upm-package

rm -r "$PACKAGE_DIR/Plugins/Android/Kotlin"
rm "$PACKAGE_DIR/Plugins/Android/Kotlin.meta"

cp EDM/BugsnagAndroidDependencies.xml "$PACKAGE_DIR/Editor"
cp EDM/BugsnagAndroidDependencies.xml.meta "$PACKAGE_DIR/Editor"

sed -i '' "s/Bugsnag SDK for Unity/Bugsnag SDK for Unity Including EDM4U Support/g" "$PACKAGE_DIR/README.md"





