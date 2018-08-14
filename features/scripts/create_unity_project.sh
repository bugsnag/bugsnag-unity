#!/bin/sh -ex

export PATH="$PATH:/Applications/Unity/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  pushd ../..
    package_path=`pwd`
  popd
  pushd ../fixtures
    rm -rf unity_project
    Unity -nographics -quit -batchmode -logFile unity.log -createProject unity_project
    Unity -nographics -quit -batchmode -logFile unity.log -projectpath unity_project -importPackage "$package_path/Bugsnag.unitypackage"
    cp Main.cs unity_project/Assets/Main.cs
    BUGSNAG_APIKEY=a35a2a72bd230ac0aa0f52715bbdc6aa Unity -nographics -quit -batchmode -logFile unity.log -projectpath unity_project -executeMethod "Main.CreateScene"
    Unity -nographics -quit -batchmode -logFile unity.log -projectpath unity_project -buildOSXUniversalPlayer "$package_path/features/fixtures/Mazerunner.app"
  popd
popd
