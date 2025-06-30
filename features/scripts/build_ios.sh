#!/usr/bin/env bash
set -euo pipefail

# === Validate input ===
if [[ -z "${UNITY_VERSION:-}" ]]; then
  echo "❌ UNITY_VERSION must be set (e.g. 2021.3.45f1)"
  exit 1
fi

if [[ $# -lt 1 ]]; then
  echo "❌ Build type must be specified: 'release' or 'dev'"
  exit 1
fi

BUILD_TYPE="$1"

if [[ "$BUILD_TYPE" != "dev" && "$BUILD_TYPE" != "release" ]]; then
  echo "❌ Invalid build type specified: must be 'release' or 'dev'"
  exit 1
fi

# === Resolve paths ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FIXTURE_DIR="$SCRIPT_DIR/../fixtures"
PROJECT_PATH="$FIXTURE_DIR/maze_runner"
ARCHIVE_PATH="$PROJECT_PATH/archive/Unity-iPhone.xcarchive"
OUTPUT_DIR="$PROJECT_PATH/output"
IPA_SUFFIX="${UNITY_VERSION:0:4}"

if [[ "$BUILD_TYPE" == "dev" ]]; then
  XCODE_PROJECT="mazerunner_dev_xcode/Unity-iPhone.xcodeproj"
  OUTPUT_IPA="mazerunner_dev_${IPA_SUFFIX}.ipa"
else
  XCODE_PROJECT="mazerunner_xcode/Unity-iPhone.xcodeproj"
  OUTPUT_IPA="mazerunner_${IPA_SUFFIX}.ipa"
fi

# === Clean old builds ===
echo "🧹 Cleaning previous IPA builds..."
if [ -d "$OUTPUT_DIR" ]; then
  find "$OUTPUT_DIR" -name "*.ipa" -exec rm -f {} +
else
  echo "ℹ️ Output directory does not exist, skipping clean."
fi

# === Archive build ===
echo "📦 Archiving Xcode project ($BUILD_TYPE)..."
xcrun xcodebuild \
  -project "$PROJECT_PATH/$XCODE_PROJECT" \
  -scheme Unity-iPhone \
  -configuration Release \
  clean archive \
  -archivePath "$ARCHIVE_PATH" \
  -allowProvisioningUpdates \
  -allowProvisioningDeviceRegistration \
  -quiet \
  GCC_WARN_INHIBIT_ALL_WARNINGS=YES

echo "✅ Archive complete"

# === Export IPA ===
echo "📤 Exporting IPA from archive..."
xcrun xcodebuild -exportArchive \
  -archivePath "$ARCHIVE_PATH" \
  -exportPath "$OUTPUT_DIR" \
  -exportOptionsPlist "$SCRIPT_DIR/exportOptions.plist" \
  -quiet

echo "✅ Export complete"

# === Rename and move IPA ===
echo "📁 Moving IPA to final destination..."
IPA_PATH="$(find "$OUTPUT_DIR" -name "*.ipa" | head -n 1)"
if [[ -z "$IPA_PATH" ]]; then
  echo "❌ Failed to locate exported .ipa"
  exit 1
fi

mv "$IPA_PATH" "$PROJECT_PATH/$OUTPUT_IPA"

echo "🎉 Successfully built and exported: $PROJECT_PATH/$OUTPUT_IPA"
