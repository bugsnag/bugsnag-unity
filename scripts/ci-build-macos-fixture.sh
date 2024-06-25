#!/bin/bash -e

if [ -z "$UNITY_VERSION" ]; then
  echo "UNITY_VERSION must be set, to e.g. 2018.4.36f1"
  exit 1
fi

if [[ $# != 1 ]]; then
  echo "Build type (release/dev) must be passed as a parameter"
  exit 2
fi

BUILD_TYPE=$1

# Build the MacOS and WebGL fixtures
./features/scripts/build_maze_runner.sh $BUILD_TYPE macos

pushd features/fixtures/maze_runner/build
  zip -r MacOS-${UNITY_VERSION:0:4}.zip MacOS
popd
