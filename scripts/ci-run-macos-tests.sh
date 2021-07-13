#!/bin/bash
pushd features/fixtures/maze_runner/build
  unzip MacOS-$UNITY_VERSION.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos
