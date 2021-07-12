#!/bin/bash
pushd features/fixtures/maze_runner
  unzip Mazerunner-$UNITY_VERSION.app.zip
  pushd ../../..
    bundle install
    bundle exec maze-runner --app=features/fixtures/maze_runner/Mazerunner.app --os=macos
  popd
popd
