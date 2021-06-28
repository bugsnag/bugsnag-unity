#!/usr/bin/env sh

if [[ "$BUILDKITE_MESSAGE" == *"[full ci]"* ||
  "$BUILDKITE_BRANCH" == "next" ||
  "$BUILDKITE_BRANCH" == "master" ||
  "$BUILDKITE_PULL_REQUEST_BASE_BRANCH" == "master" ]]; then
  echo "Running full build"
  buildkite-agent pipeline upload .buildkite/pipeline.full.yml
fi
