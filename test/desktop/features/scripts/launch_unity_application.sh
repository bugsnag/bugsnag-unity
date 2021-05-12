#!/bin/sh -ex

pushd "${0%/*}"
  pushd ../fixtures/maze_runner
    ${UNITY_PROJECT_NAME}.app/Contents/MacOS/${UNITY_PROJECT_NAME} -batchmode -nographics
  popd
popd
