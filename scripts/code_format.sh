#!/usr/bin/env bash
set -Eeuo pipefail

# --- Config ---
PROJECT_DIR="Bugsnag"
SLN_PATH="$PROJECT_DIR/Bugsnag.sln"
UNITY_BIN="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
UNITY_LOG="${PROJECT_DIR}/Library/Logs/SyncVS-$(date +%Y%m%d-%H%M%S).log"

DEFAULT_CLI_ARGS=(-batchmode -nographics -logFile "$UNITY_LOG")
UNITY_PID=""

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

# Always delete old solution files to force regeneration
echo "    Removing old solution files to force regeneration..."
rm -f "$PROJECT_DIR/"*.sln "$PROJECT_DIR/"*.csproj

cleanup_unity() {
  if [[ -n "$UNITY_PID" ]] && kill -0 "$UNITY_PID" 2>/dev/null; then
    echo "    Stopping Unity (PID: $UNITY_PID)"
    kill "$UNITY_PID" 2>/dev/null || true
    wait "$UNITY_PID" 2>/dev/null || true
  fi
}

trap cleanup_unity EXIT

attempt_sync() {
  "$UNITY_BIN" "${DEFAULT_CLI_ARGS[@]}" \
    -projectPath "$PROJECT_DIR" &
  UNITY_PID=$!
  echo "    Unity started (PID: $UNITY_PID)"
}

wait_for_solution() {
  # Wait up to MAX_WAIT seconds for .sln and at least one .csproj
  local MAX_WAIT=60
  local waited=0
  echo "    Waiting for Unity to generate solution files..."
  while (( waited < MAX_WAIT )); do
    # Check if Unity is still running
    if [[ -n "$UNITY_PID" ]] && ! kill -0 "$UNITY_PID" 2>/dev/null; then
      echo "    Unity process exited early"
      return 1
    fi
    
    if [[ -f "$SLN_PATH" ]] && compgen -G "$PROJECT_DIR/"'*.csproj' > /dev/null; then
      return 0
    fi
    sleep 1
    (( waited+=1 ))
    if (( waited % 10 == 0 )); then
      echo "    ... still waiting (${waited}s elapsed)"
    fi
  done
  return 1
}

# --- Sync with retry ---
SUCCESS=0
SYNC_ATTEMPTS=1
for attempt in $(seq 1 "$SYNC_ATTEMPTS"); do
  echo "---- Sync attempt $attempt/$SYNC_ATTEMPTS"
  attempt_sync

  if wait_for_solution; then
    echo "    ✔ Solution generated: $SLN_PATH"
    cleanup_unity
    SUCCESS=1
    break
  fi

  cleanup_unity
  echo "    ✖ Solution files not found within timeout"
done

# --- Validate artifacts before continuing ---
if [[ $SUCCESS -eq 0 ]]; then
  echo "✖ Solution file not found after retries: $SLN_PATH" >&2
  echo "---- Unity log tail ----" >&2
  tail -n 300 "$UNITY_LOG" || true
  exit 2
fi

# --- dotnet format (solution mode) ---
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