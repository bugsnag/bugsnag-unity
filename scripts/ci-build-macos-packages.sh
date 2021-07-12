#!/bin/bash

# Build the MacOS and WebGL fixtures
./features/scripts/build_maze_runner.sh macos
if [[ $? != 0 ]]; then
  exit $1
fi

./features/scripts/build_maze_runner.sh webgl
if [[ $? != 0 ]]; then
  exit $1
fi

pushd features/fixtures/maze_runner/build
  zip -r MacOS-$UNITY_VERSION.zip MacOS
  zip -r WebGL-$UNITY_VERSION.zip WebGL
popd
