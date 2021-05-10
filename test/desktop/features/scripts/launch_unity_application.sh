#!/bin/sh -ex

pushd "${0%/*}"
  pushd ../fixtures/maze_runner
    Mazerunner.app/Contents/MacOS/Mazerunner -batchmode -nographics
  popd
popd
