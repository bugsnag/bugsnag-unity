REM Using the artifacts plugin v1.3 on Windows seems to break the whole step
buildkite-agent artifact download "Bugsnag.unitypackage" .
cd features\scripts
C:\Progra~1\Git\bin\bash.exe build_maze_runner.sh
cd ..\fixtures\maze_runner
7z a -mx9 -r WindowsBuild-%UNITY_VERSION%.zip WindowsBuild
