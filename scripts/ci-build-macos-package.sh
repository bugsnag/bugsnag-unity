#!/bin/bash
./features/scripts/build_maze_runner.sh
if [[ $? != 0 ]]; then
  popd
  exit $1
fi

pushd features/fixtures/maze_runner
  zip -r Mazerunner-$UNITY_VERSION.app.zip Mazerunner.app
popd
