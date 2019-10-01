#!/bin/sh -ex

pushd "${0%/*}"
  pushd ../fixtures/unity_project
    Mazerunner.app/Contents/MacOS/Mazerunner -batchmode -nographics
  popd
popd
