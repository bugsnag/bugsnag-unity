#!/bin/bash -e

# Build the MacOS and WebGL fixtures
./features/scripts/build_maze_runner.sh macos
./features/scripts/build_maze_runner.sh webgl

pushd features/fixtures/maze_runner/build
  zip -r MacOS-${UNITY_VERSION:0:4}.zip MacOS
  zip -r WebGL-${UNITY_VERSION:0:4}.zip WebGL
popd
