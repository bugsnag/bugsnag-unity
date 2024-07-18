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
    unzip WebGL-release-${UNITY_VERSION:0:4}.zip
  else
    unzip WebGL-dev-${UNITY_VERSION:0:4}.zip
  fi
popd

bundle install
# TODO: WebGL persistence tests are currently skipped pending PLAT-8151
bundle exec maze-runner --farm=local --browser=firefox -e features/csharp/csharp_persistence.feature features/csharp
