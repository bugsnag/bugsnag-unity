#!/bin/bash
pushd features/fixtures/maze_runner
  unzip MacOS-$UNITY_VERSION.zip
  pushd ../../..
    bundle install
    bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos
    RESULT=$?
  popd
popd
exit $RESULT
