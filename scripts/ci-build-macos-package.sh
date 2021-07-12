#!/bin/bash
./features/scripts/build_maze_runner.sh macos
if [[ $? != 0 ]]; then
  exit $1
fi

pushd features/fixtures/maze_runner/build
  zip -r MacOS-$UNITY_VERSION.zip macos
popd
