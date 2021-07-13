REM Using the artifacts plugin v1.3 on Windows seems to break the whole step
buildkite-agent artifact download "features\fixtures\maze_runner\build\Windows-%UNITY_VERSION%.zip" .
cd features\fixtures\maze_runner\build
7z x Windows-%UNITY_VERSION%.zip
cd ..\..\..\..
wsl -d Ubuntu bundle install
wsl -d Ubuntu bundle exec maze-runner --app=features/fixtures/maze_runner/build/Windows/Mazerunner.exe --os=windows
