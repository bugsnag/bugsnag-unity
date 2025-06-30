#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# === Validate inputs ===
if [[ $# -ne 2 ]]; then
  echo "❌ Usage: $0 <Xcode project path> <export name>"
  exit 1
fi

readonly XCODE_PROJECT_PATH="$1"
readonly EXPORT_NAME="$2"

echo "📁 Xcode project path: $XCODE_PROJECT_PATH"
echo "🚚 Export name: $EXPORT_NAME"

# === Paths & Constants ===
readonly OUTPUT_DIR="$XCODE_PROJECT_PATH/output"
readonly ARCHIVE_DIR="$XCODE_PROJECT_PATH/archive"
readonly ARCHIVE_PATH="$ARCHIVE_DIR/Unity-iPhone.xcarchive"
readonly EXPORT_OPTIONS_PLIST="features/scripts/exportOptions.plist"
readonly FINAL_IPA_DEST="features/fixtures/minimalapp/${EXPORT_NAME}.ipa"

# === Clean previous builds ===
echo "🧹 Cleaning previous .ipa files..."
if [[ -d "$OUTPUT_DIR" ]]; then
  find "$OUTPUT_DIR" -name "*.ipa" -exec rm -f {} +
else
  echo "ℹ️ Output directory not found; skipping clean."
fi

# === Archive the project ===
echo "📦 Archiving project..."
xcrun xcodebuild \
  -project "$XCODE_PROJECT_PATH/Unity-iPhone.xcodeproj" \
  -scheme Unity-iPhone \
  -configuration Debug \
  -archivePath "$ARCHIVE_PATH" \
  -allowProvisioningUpdates \
  -allowProvisioningDeviceRegistration \
  -quiet \
  GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
  archive

echo "✅ Archive succeeded"

# === Export the .ipa ===
echo "📤 Exporting archive to .ipa..."
xcrun xcodebuild -exportArchive \
  -archivePath "$ARCHIVE_PATH" \
  -exportPath "$OUTPUT_DIR" \
  -exportOptionsPlist "$EXPORT_OPTIONS_PLIST" \
  -quiet

echo "✅ Export succeeded"

# === Move IPA to known location ===
echo "🚚 Moving .ipa to final destination..."
IPA_PATH=$(find "$OUTPUT_DIR" -name "*.ipa" | head -n 1)

if [[ -z "$IPA_PATH" ]]; then
  echo "❌ No .ipa found in $OUTPUT_DIR"
  exit 1
fi

mv -f "$IPA_PATH" "$FINAL_IPA_DEST"
echo "🎉 Build complete: $FINAL_IPA_DEST"
