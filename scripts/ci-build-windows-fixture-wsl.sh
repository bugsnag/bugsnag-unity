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

cd features/scripts
./build_maze_runner.sh $BUILD_TYPE wsl
cd ../fixtures/maze_runner/build

if [ "$BUILD_TYPE" == "release" ]; then
  zip -r Windows-release-${UNITY_VERSION:0:4}.zip Windows
else
  zip -r Windows-dev-${UNITY_VERSION:0:4}.zip Windows
fi
