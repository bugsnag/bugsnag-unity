#!/bin/bash -e
cd features/fixtures/maze_runner/build
unzip Windows-$(UNITY_VERSION).zip
cd ../../../..
bundle install
bundle exec maze-runner --app=features/fixtures/maze_runner/build/Windows/Mazerunner.exe --os=windows features/desktop
