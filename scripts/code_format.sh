#!/usr/bin/env bash
set -Eeuo pipefail

# --- Config ---
PROJECT_DIR="Bugsnag"
SLN_PATH="$PROJECT_DIR/Bugsnag.sln"
UNITY_BIN="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
UNITY_LOG="${PROJECT_DIR}/Library/Logs/SyncVS-$(date +%Y%m%d-%H%M%S).log"

DEFAULT_CLI_ARGS=(-quit -batchmode -nographics -logFile "$UNITY_LOG")
SYNC_METHOD="UnityEditor.SyncVS.SyncSolution"

# --- Preflight ---
if [[ -z "${UNITY_VERSION:-}" ]]; then
  echo "UNITY_VERSION must be set" >&2
  exit 1
fi
if [[ ! -x "$UNITY_BIN" ]]; then
  echo "Unity not found at: $UNITY_BIN" >&2
  exit 1
fi
if [[ ! -d "$PROJECT_DIR" ]]; then
  echo "Project directory not found: $PROJECT_DIR" >&2
  exit 1
fi

echo "==> Generating solution via Unity (${UNITY_VERSION})"
echo "    Log: $UNITY_LOG"

# --- Run SyncVS with a retry (Unity can be moody on cold caches) ---
attempt_sync() {
  "$UNITY_BIN" "${DEFAULT_CLI_ARGS[@]}" \
    -projectPath "$PROJECT_DIR" \
    -executeMethod "$SYNC_METHOD"
}

SYNC_ATTEMPTS=2
for attempt in $(seq 1 "$SYNC_ATTEMPTS"); do
  echo "---- Sync attempt $attempt/$SYNC_ATTEMPTS"
  set +e
  attempt_sync
  UNITY_EXIT=$?
  set -e

  # Poll for .sln and at least one .csproj (up to 30s)
  FOUND_SOLUTION=0
  for i in {1..30}; do
    if [[ -f "$SLN_PATH" ]] && compgen -G "$PROJECT_DIR/*.csproj" > /dev/null; then
      FOUND_SOLUTION=1
      break
    fi
    sleep 1
  done

  if [[ "$UNITY_EXIT" -eq 0 && "$FOUND_SOLUTION" -eq 1 ]]; then
    echo "    ✔ Solution generated: $SLN_PATH"
    break
  fi

  echo "    Waiting for files failed or Unity exit was $UNITY_EXIT. Retrying..."
done

# --- Validate artifacts before continuing ---
if [[ ! -f "$SLN_PATH" ]]; then
  echo "✖ Solution file not found after retries: $SLN_PATH" >&2
  echo "---- Unity log tail ----" >&2
  tail -n 200 "$UNITY_LOG" || true
  exit 2
fi
if ! compgen -G "$PROJECT_DIR/*.csproj" > /dev/null; then
  echo "✖ No .csproj files found under $PROJECT_DIR (required by dotnet format)." >&2
  echo "---- Unity log tail ----" >&2
  tail -n 200 "$UNITY_LOG" || true
  exit 3
fi

# --- Decide dotnet format mode ---
FORMAT_ARGS=( --no-restore )
if [[ "${1:-}" == "--verify" ]]; then
  FORMAT_ARGS+=( --verify-no-changes )
fi

echo "==> Running dotnet format on solution"
echo "    dotnet format ${FORMAT_ARGS[*]} \"$SLN_PATH\""
set +e
dotnet format "${FORMAT_ARGS[@]}" "$SLN_PATH"
EXIT_CODE=$?
set -e

if [[ "$EXIT_CODE" -ne 0 ]]; then
  echo "✖ Code formatting verification found issues (exit $EXIT_CODE)." >&2
  exit "$EXIT_CODE"
fi

echo "✔ Done."