REM Using the artifacts plugin v1.3 on Windows seems to break the whole step
buildkite-agent artifact download "Bugsnag.unitypackage" .
cd features\scripts
C:\Progra~1\Git\bin\bash.exe build_maze_runner.sh windows
IF NOT [%ERRORLEVEL%] EQU [0] EXIT /B %ERRORLEVEL%

cd ..\fixtures\maze_runner\build
7z a -r Windows-%UNITY_VERSION%.zip Windows
