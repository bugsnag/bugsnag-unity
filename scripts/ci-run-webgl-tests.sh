#!/bin/bash -e

pushd features/fixtures/maze_runner/build
  unzip WebGL-$UNITY_VERSION.zip
popd

bundle install
bundle exec maze-runner --farm=local --browser=firefox features/desktop
