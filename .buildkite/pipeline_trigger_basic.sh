#!/usr/bin/env sh

if [[ "$BUILDKITE_MESSAGE" == *"[basic ci]"* ||
  "$BUILDKITE_MESSAGE" == *"[full ci]"* ||
  "$BUILDKITE_BRANCH" == "next" ||
  "$BUILDKITE_BRANCH" == "master" ||
  "$BUILDKITE_BRANCH" == releases/* ||
  ! -z "$FULL_SCHEDULED_BUILD" ||
  "$BUILDKITE_PULL_REQUEST_BASE_BRANCH" == "master" ]]; then
  echo "Running basic build"
  buildkite-agent pipeline upload .buildkite/pipeline.basic.yml
else
  # Allow basic (and later full) builds to be triggered
  echo "Running zero build"
  buildkite-agent pipeline upload .buildkite/block.basic.yml
fi
