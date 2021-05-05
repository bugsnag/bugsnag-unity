#!/bin/sh -ex

pushd "${0%/*}"
  pushd ../fixtures
    ${UNITY_TEST_PROJECT}/Contents/MacOS/Mazerunner -batchmode -nographics
  popd
popd
