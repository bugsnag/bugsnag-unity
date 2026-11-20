#!/usr/bin/env bash
set -eo pipefail

if [ -z "$UNITY_VERSION" ]
then
  echo "UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"

pushd "${0%/*}" >/dev/null
  script_path=$(pwd)
popd >/dev/null

project_path="$script_path/../fixtures/maze_runner"
XCODE_PROJECT="$project_path/mazerunner_simulator_xcode/Unity-iPhone.xcodeproj"

# Import the Bugsnag package into the fixture
"$script_path/prepare_fixture.sh"

# Generate the Xcode project with the Simulator SDK and arm64 architecture selected
# (built twice for the same -ObjC linker flag workaround as generate_xcode_project.sh)
for _ in 1 2; do
  "$UNITY_PATH/Unity" -quit -nographics -batchmode -logFile "$script_path/../fixtures/unity.log" \
    -buildTarget iOS -projectPath "$project_path" -executeMethod Builder.IosSimulator
done

# Build for the arm64 simulator only; this fails if the XCFramework lacks an arm64 simulator slice
xcrun xcodebuild \
  -project "$XCODE_PROJECT" \
  -scheme Unity-iPhone \
  -configuration Release \
  -sdk iphonesimulator \
  -destination "generic/platform=iOS Simulator" \
  ARCHS=arm64 \
  ONLY_ACTIVE_ARCH=NO \
  CODE_SIGNING_ALLOWED=NO \
  GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
  -quiet \
  build

echo "Successfully built the arm64 iOS simulator fixture"
