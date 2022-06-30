#!/bin/bash -e
cd features/scripts
./build_maze_runner.sh wsl
cd ../fixtures/maze_runner/build
zip -r Windows-$UNITY_VERSION.zip Windows
