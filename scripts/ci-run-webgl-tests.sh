#!/bin/bash -e

pushd features/fixtures/maze_runner/build
  unzip WebGL-$UNITY_VERSION.zip
popd

bundle install
# TODO: WebGL persistence tests are currently skipped pending PLAT-8151
bundle exec maze-runner --farm=local --browser=firefox -e features/csharp
