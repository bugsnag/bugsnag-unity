#!/bin/sh -ex

export PATH="$PATH:/Applications/Unity/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  pushd ../..
    package_path=`pwd`
  popd
  pushd ../fixtures
    git clean -xdf .
    project_path="$(pwd)/unity_project"
    Unity -nographics -quit -batchmode -logFile unity.log -createProject $project_path
    Unity -nographics -quit -batchmode -logFile unity.log -projectpath $project_path -importPackage "$package_path/Bugsnag.unitypackage"
    cp Main.cs unity_project/Assets/Main.cs
    BUGSNAG_APIKEY=a35a2a72bd230ac0aa0f52715bbdc6aa Unity -nographics -quit -batchmode -logFile unity.log -projectpath $project_path -executeMethod "Main.CreateScene"
    Unity -nographics -quit -batchmode -logFile unity.log -projectpath $project_path -buildOSXUniversalPlayer "$package_path/features/fixtures/Mazerunner.app"
  popd
popd
