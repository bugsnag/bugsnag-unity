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

# Build the WebGL fixture
./features/scripts/build_maze_runner.sh $BUILD_TYPE webgl

pushd features/fixtures/maze_runner/build
  if [ "$BUILD_TYPE" == "release" ]; then
    FIXTURE_NAME=Mazerunner
    BUILD_FOLDER=WebGL-release-${UNITY_VERSION:0:4}
  else
    FIXTURE_NAME=Mazerunner_dev
    BUILD_FOLDER=WebGL-dev-${UNITY_VERSION:0:4}
  fi
  zip -r "${BUILD_FOLDER}.zip" WebGL
  # Check if index.html exists in the build folder
  if [ ! -f "WebGL/${FIXTURE_NAME}/index.html" ]; then
    echo "index.html not found in the build folder"
    exit 3
  fi
popd
