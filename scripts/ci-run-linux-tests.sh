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

pushd features/fixtures/maze_runner/build
  if [ "$BUILD_TYPE" == "release" ]; then
    unzip linux-release-${UNITY_VERSION:0:4}.zip
    APP_PATH="features/fixtures/maze_runner/build/linux/Mazerunner"
  else
    unzip linux-dev-${UNITY_VERSION:0:4}.zip
    APP_PATH="features/fixtures/maze_runner/build/linux/Mazerunner_dev"
  fi
popd

bundle install
bundle exec maze-runner --app=$APP_PATH --os=linux features/csharp