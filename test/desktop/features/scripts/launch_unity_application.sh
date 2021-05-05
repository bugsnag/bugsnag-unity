#!/bin/sh -ex

pushd "${0%/*}"
  pushd ../fixtures
    Mazerunner.app/Contents/MacOS/Mazerunner -batchmode -nographics
  popd
popd
