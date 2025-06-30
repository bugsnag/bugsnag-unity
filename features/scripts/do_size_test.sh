#!/usr/bin/env bash
set -euo pipefail

if [ -z "${UNITY_VERSION:-}" ]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
PROJECT_PATH="features/fixtures/minimalapp"
PACKAGE_PATH="Bugsnag.unitypackage"
IMPORTED_SDK_PATH="$PROJECT_PATH/Assets/Bugsnag"

echo "📦 Installing required gems..."
bundle install

echo "🧹 Removing any existing Bugsnag SDK import..."
rm -rf "$IMPORTED_SDK_PATH"

echo "📱 Building Android (without Bugsnag)..."
"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
  -projectPath "$PROJECT_PATH" \
  -executeMethod Builder.BuildAndroidWithout \
  -logFile build_android_minimal_without.log

echo "🍎 Building iOS (without Bugsnag)..."
"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
  -projectPath "$PROJECT_PATH" \
  -executeMethod Builder.BuildIosWithout \
  -logFile export_ios_xcode_project_minimal_without.log

echo "🏗️ Building Xcode project (without Bugsnag)..."
source ./features/scripts/build_xcode_project.sh "$PROJECT_PATH/minimal_without_xcode" without_bugsnag

echo "📦 Re-importing Bugsnag SDK..."
mv -f "$PACKAGE_PATH" "$PROJECT_PATH"

"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
  -projectPath "$PROJECT_PATH" \
  -ignoreCompilerErrors \
  -importPackage "$PROJECT_PATH/$(basename $PACKAGE_PATH)"

echo "📱 Building Android (with Bugsnag)..."
"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
  -projectPath "$PROJECT_PATH" \
  -executeMethod Builder.BuildAndroidWith \
  -logFile build_android_minimal_with.log

echo "🍎 Building iOS (with Bugsnag)..."
"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
  -projectPath "$PROJECT_PATH" \
  -executeMethod Builder.BuildIosWith \
  -logFile export_ios_xcode_project_minimal_with.log

echo "🏗️ Building Xcode project (with Bugsnag)..."
source ./features/scripts/build_xcode_project.sh "$PROJECT_PATH/minimal_with_xcode" with_bugsnag

echo "🚨 Running Danger..."
cd "$PROJECT_PATH"
bundle install
bundle exec danger
