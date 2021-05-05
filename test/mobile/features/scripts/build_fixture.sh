#!/usr/bin/env bash

pushd "${0%/*}"
  script_path=`pwd`
popd

echo "Script in: $script_path"

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output, disable the
# package manager (we just don't need it and it slows things down)
DEFAULT_CLI_ARGS="-quit -batchmode -logFile unity.log -noUpm"

# Creating a new project in the MyProject directory
Unity $DEFAULT_CLI_ARGS -createProject `pwd`/unity_project

# Installing the Bugsnag package
Unity $DEFAULT_CLI_ARGS -importPackage $script_path/../../../../Bugsnag.unitypackage

cp Assets/Editor/Builder.cs unity_project/Assets/Builder.cs

# Running a custom script - must reference a static method
Unity $DEFAULT_CLI_ARGS -executeMethod Builder.AndroidBuild -projectPath `pwd`/unity_project

## Open unity.log to see the Hello World
open unity.log
