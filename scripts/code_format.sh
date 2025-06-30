#!/usr/bin/env bash
set -euo pipefail

# === Check for UNITY_VERSION ===
if [ -z "${UNITY_VERSION:-}" ]; then
  echo "❌ UNITY_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS"
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
PROJECT_PATH="Bugsnag"
SOLUTION_PATH="$PROJECT_PATH/Bugsnag.sln"

# === Generate .sln and project files ===
echo "📁 Generating solution files with Unity..."
"$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS -projectPath "$PROJECT_PATH" -executeMethod "UnityEditor.SyncVS.SyncSolution"

# === Choose format command ===
FORMAT_COMMAND="dotnet format"
if [ "${1:-}" == "--verify" ]; then
  FORMAT_COMMAND+=" --verify-no-changes"
  echo "🔍 Verifying code formatting..."
else
  echo "🧹 Applying code formatting..."
fi

# === Run dotnet format ===
$FORMAT_COMMAND "$SOLUTION_PATH"
EXIT_CODE=$?

if [ $EXIT_CODE -ne 0 ]; then
  echo "❌ Code formatting issues detected."
  exit $EXIT_CODE
fi

echo "✅ Done. Code formatting is correct."
