#!/bin/sh -ex

export PATH="$PATH:/Applications/Unity/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  pushd ../..
    package_path=`pwd`
  popd
  pushd ../fixtures
    git clean -xdf .
    log_file="$package_path/unity.log"
    project_path="$(pwd)/unity_project"

    Unity -nographics -quit -batchmode -logFile $log_file \
      -createProject $project_path

    Unity -nographics -quit -batchmode -logFile $log_file \
      -projectPath $project_path \
      -importPackage "$package_path/Bugsnag.unitypackage"

    cp Main.cs unity_project/Assets/Main.cs
    cp -R NativeCrashy.bundle unity_project/Assets/Plugins/OSX/NativeCrashy.bundle

    Unity -nographics -quit -batchmode -logFile $log_file \
      -projectPath $project_path \
      -executeMethod "Main.CreateScene"

    Unity -nographics -quit -batchmode -logFile $log_file \
      -projectPath $project_path \
      -buildOSXUniversalPlayer "$(pwd)/Mazerunner.app"
  popd
popd
