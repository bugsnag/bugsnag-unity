#!/bin/sh -ex

pushd "${0%/*}"
  pushd ../fixtures
    ${UNITY_PROJECT_NAME}.app/Contents/MacOS/${UNITY_PROJECT_NAME} -batchmode -nographics
  popd
popd
