#!/bin/bash -e
pushd features/fixtures/maze_runner/build
  unzip MacOS-${UNITY_VERSION:0:4}.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos features/macos features/csharp
