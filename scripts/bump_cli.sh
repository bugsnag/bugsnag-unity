#!/usr/bin/env bash
set -euo pipefail

# Usage: ./bump_cli.sh NEW_VERSION
# e.g., ./bump_cli.sh 3.0.0

if [ $# -lt 1 ]; then
  echo "Error: Please provide a version (e.g., ./bump_cli.sh 3.0.0)" >&2
  exit 1
fi

NEW_VERSION="$1"
CLI_FILE="Bugsnag/Assets/Bugsnag/Editor/SymbolUpload/BugsnagCLI.cs"

if [ ! -f "$CLI_FILE" ]; then
  echo "Error: '$CLI_FILE' does not exist." >&2
  exit 1
fi

# Read the file contents
OLD_CONTENT="$(< "$CLI_FILE")"

# Use sed to replace the line matching DOWNLOADED_CLI_VERSION
NEW_CONTENT="$(echo "$OLD_CONTENT" | \
  sed -E "s/private const string DOWNLOADED_CLI_VERSION = \".*\";/private const string DOWNLOADED_CLI_VERSION = \"$NEW_VERSION\";/g")"

# Write the updated content back to the file
echo "$NEW_CONTENT" > "$CLI_FILE"

echo "Successfully updated DOWNLOADED_CLI_VERSION to '$NEW_VERSION' in $CLI_FILE"