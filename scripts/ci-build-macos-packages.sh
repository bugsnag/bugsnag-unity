#!/bin/bash

# Build the MacOS and WebGL fixtures
./features/scripts/build_maze_runner.sh macos
CODE=$?
if [[ $CODE != 0 ]]; then
  echo "Error, exit code: $CODE"
  exit $CODE
fi

./features/scripts/build_maze_runner.sh webgl
CODE=$?
if [[ $? != 0 ]]; then
  echo "Error, exit code: $CODE"
  exit $CODE
fi

pushd features/fixtures/maze_runner/build
  zip -r MacOS-$UNITY_VERSION.zip MacOS
  zip -r WebGL-$UNITY_VERSION.zip WebGL
popd
