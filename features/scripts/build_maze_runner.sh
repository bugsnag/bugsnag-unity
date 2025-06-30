#!/bin/bash -e

# === Validate input ===
if [ -z "$UNITY_VERSION" ]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

if [ $# -ne 2 ]; then
  echo "❌ Usage: $0 <release|dev> <macos|webgl|windows|wsl>"
  exit 2
fi

BUILD_TYPE="$1"
PLATFORM_TYPE="$2"

# === Determine platform-specific config ===
case "$PLATFORM_TYPE" in
  macos)
    PLATFORM="MacOS$(echo "$BUILD_TYPE" | awk '{print toupper(substr($0,1,1)) tolower(substr($0,2))}')"
    UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
    ;;
  windows)
    PLATFORM="Win64$(echo "$BUILD_TYPE" | awk '{print toupper(substr($0,1,1)) tolower(substr($0,2))}')"
    UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
    set -m
    ;;
  wsl)
    PLATFORM="Win64$(echo "$BUILD_TYPE" | awk '{print toupper(substr($0,1,1)) tolower(substr($0,2))}')"
    UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
    set -m
    ;;
  webgl)
    PLATFORM="WebGL$(echo "$BUILD_TYPE" | awk '{print toupper(substr($0,1,1)) tolower(substr($0,2))}')"
    if [[ "$(uname)" == "Darwin" ]]; then
      UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
    else
      UNITY_PATH="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
    fi
    ;;
  *)
    echo "❌ Unsupported platform: $PLATFORM_TYPE"
    exit 3
    ;;
esac

# === Prepare paths ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
FIXTURES_DIR="$REPO_ROOT/features/fixtures"
PROJECT_PATH="$FIXTURES_DIR/maze_runner"
PACKAGE_PATH="$REPO_ROOT/Bugsnag.unitypackage"
IMPORT_LOG_FILE="$REPO_ROOT/unity_import.log"
BUILD_LOG_FILE="$REPO_ROOT/unity.log"

# === Handle WSL paths if needed ===
if [ "$PLATFORM_TYPE" == "wsl" ]; then
  touch "$IMPORT_LOG_FILE" "$BUILD_LOG_FILE"  # prevent wslpath failure
  IMPORT_LOG_FILE=$(wslpath -w "$IMPORT_LOG_FILE")
  BUILD_LOG_FILE=$(wslpath -w "$BUILD_LOG_FILE")
  PACKAGE_PATH=$(wslpath -w "$PACKAGE_PATH")
  PROJECT_PATH=$(wslpath -w "$PROJECT_PATH")
fi

# === Import package ===
echo "📦 Importing $PACKAGE_PATH into $PROJECT_PATH"
"$UNITY_PATH" -nographics -quit -batchmode \
  -logFile "$IMPORT_LOG_FILE" \
  -projectPath "$PROJECT_PATH" \
  -ignoreCompilerErrors \
  -importPackage "$PACKAGE_PATH"

# === Build project ===
echo "🏗️  Building project for platform: $PLATFORM"
"$UNITY_PATH" -nographics -quit -batchmode \
  -logFile "$BUILD_LOG_FILE" \
  -projectPath "$PROJECT_PATH" \
  -executeMethod "Builder.$PLATFORM"

echo "✅ Build completed successfully for $PLATFORM_TYPE ($BUILD_TYPE)"
