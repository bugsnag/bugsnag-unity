#!/bin/bash

if [ -z "$1" ]
then
  echo "ERROR: Project path not set, please pass the path to your unity project as the first argument"
  exit 1
fi

PROJECT_PATH=$1

cd $PROJECT_PATH

find . -iname "*bugsnag*" -delete

rm -f "Assets/Plugins/Android/kotlin-stdlib-common.jar"
rm -f "Assets/Plugins/Android/kotlin-annotations.jar"
rm -f "Assets/Plugins/Android/kotlin-stdlib.jar"
rm -r -f "Assets/Plugins/OSX/Bugsnag"
