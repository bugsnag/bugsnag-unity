# Bugsnag Unity SDK — Developer Overview

This document gives a medium-level view of how the repository is structured, how the SDK is architected, and how to develop, build, and test changes. For installation instructions see [README.md](README.md). For a detailed release walkthrough see [CONTRIBUTING.md](CONTRIBUTING.md). For end-to-end test instructions see [TESTING.md](TESTING.md).

---

## Table of Contents

1. [Repository Layout](#repository-layout)
2. [Architecture Overview](#architecture-overview)
3. [Setting Up a Development Environment](#setting-up-a-development-environment)
4. [Build System](#build-system)
5. [Running Tests](#running-tests)
6. [Native Platform Bridges](#native-platform-bridges)
7. [Key Source Files and Concepts](#key-source-files-and-concepts)
8. [CI/CD](#cicd)
9. [Release Process](#release-process)

---

## Repository Layout

```
bugsnag-unity/
├── Bugsnag/                   # Main Unity project containing the SDK source
│   └── Assets/Bugsnag/
│       ├── Runtime/           # C# SDK code (shipped to users)
│       ├── Editor/            # Unity editor tools and build preprocessors
│       └── Tests/             # EditMode unit tests
├── upm/                       # Unity Package Manager distribution format
├── example/                   # Minimal example app for manual testing
├── features/                  # End-to-end BDD tests (Cucumber / Maze Runner)
│   ├── android/               # Android-specific feature files
│   ├── ios/                   # iOS-specific feature files
│   ├── macos/                 # macOS-specific feature files
│   ├── webgl/                 # WebGL-specific feature files
│   ├── csharp/                # C# managed exception feature files
│   ├── fixtures/              # Unity test apps built for E2E runs
│   │   └── maze_runner/       # Main fixture app (produces APK / IPA / .app)
│   ├── scripts/               # Build scripts for each platform fixture
│   └── steps/                 # Ruby BDD step definitions
├── bugsnag-android/           # Git submodule — bugsnag-android core SDK
├── bugsnag-cocoa/             # Git submodule — bugsnag-cocoa core SDK
├── bugsnag-android-unity/     # Java/Gradle bridge between Unity and bugsnag-android
├── android-libs/              # Pre-built AAR libraries for Android
├── scripts/                   # Utility and CI shell scripts
├── .buildkite/                # BuildKite CI pipeline definitions
├── Rakefile                   # Top-level build orchestration (Ruby / Rake)
├── Gemfile                    # Ruby gem dependencies
├── CHANGELOG.md
├── CONTRIBUTING.md
├── TESTING.md
└── UPGRADING.md
```

### Key top-level files

| File | Purpose |
|---|---|
| `Rakefile` | All build and test tasks — run `bundle exec rake -T` to list them |
| `Gemfile` | Ruby dependencies including Maze Runner (E2E framework) |
| `upm/package.json` | UPM package manifest (`com.bugsnag.unitynotifier`, Unity 2018.1+) |
| `CHANGELOG.md` | Version history and release notes |

---

## Architecture Overview

### Public API → Internal Client → Native Bridge

```
User code
   │
   ▼
Bugsnag (static façade)          Bugsnag.cs
   │
   ▼
Client (internal implementation) Client.cs
   │
   ├── Configuration             Configuration.cs
   ├── IBreadcrumbs              breadcrumb tracking
   ├── ISessionTracker           session lifecycle
   ├── PayloadManager            offline event queue
   ├── Delivery                  HTTP delivery with truncation
   │
   └── INativeClient ────────────────────────────────────────────┐
         │                                                        │
         ├── Native/Android/   JNI bridge → bugsnag-android      │
         ├── Native/Cocoa/     Objective-C bridge → bugsnag-cocoa  │
         ├── Native/iOS/       iOS entry point                    │
         ├── Native/MacOS/     macOS entry point                  │
         ├── Native/Windows/   Windows implementation             │
         └── Native/Fallback/  no-op for unsupported platforms ───┘
```

**`Bugsnag.cs`** is a `static` façade. All public methods simply delegate to a private `InternalClient` instance that is created by `Bugsnag.Start()`. This means there is exactly one SDK instance per Unity process.

**`Client.cs`** contains all the core logic: intercepting Unity log messages (via `ILogHandler`), building event payloads, running callbacks, managing sessions, and delivering payloads.

**`INativeClient`** abstracts platform-specific behaviour. At startup, `NativeClient` (a compile-time alias resolved per platform) instantiates the correct implementation. The native layer is responsible for capturing native crashes, reading device/OS metadata, and forwarding events to the platform SDK.

### Design Patterns in Use

| Pattern | Where |
|---|---|
| Static singleton façade | `Bugsnag` static class with a lock-protected `InternalClient` |
| Strategy / dependency injection | `INativeClient` implementations selected at compile time |
| Factory | `ErrorBuilder` constructs `Error` objects from exceptions and stack frames |
| Observer / callback chain | `AddOnError`, `AddOnSession` callback lists, invoked before delivery |
| Handler decoration | `BugsnagLogHandler` wraps Unity's existing `ILogHandler` |

### Threading Model

- Log capture and session tracking run on Unity's main thread.
- Payload delivery happens on a background thread pool.
- Shared mutable state is protected by per-object locks (`CallbackLock`, `_clientLock`, etc.).
- `_foregroundStopwatch` / `_backgroundStopwatch` track in-foreground time for automatic session thresholds.

---

## Setting Up a Development Environment

### Prerequisites

- **Unity** — install via Unity Hub. The SDK targets Unity 2018.1 and above; install any version from 2018.4 onwards to work on the C# code. The CI matrix covers 2018.4, 2020.x, 2021.x, 2022.x, and Unity 6 (2024.x).
- **Ruby** — required to run Rake build tasks and Maze Runner E2E tests (`ruby --version` ≥ 2.7 recommended).
- **Bundler** — install with `gem install bundler`, then run `bundle install` in the repo root to install Ruby dependencies.
- **Xcode** (macOS only) — required to build iOS and macOS fixtures.
- **Android SDK / NDK** — required to build Android fixtures. Follow the [bugsnag-android contributing guide](https://github.com/bugsnag/bugsnag-android/blob/master/CONTRIBUTING.md) for setup.

### Clone with submodules

```
git clone --recursive git@github.com:bugsnag/bugsnag-unity
cd bugsnag-unity
bundle install
```

If you already cloned without `--recursive`:

```
git submodule update --init --recursive
```

### Install multiple Unity versions (macOS)

A helper script installs supported Unity versions via Homebrew Cask:

```
scripts/bootstrap-unity.sh
```

To point the build at a specific Unity installation, set `UNITY_DIR`:

```
UNITY_DIR=/Applications/Unity/Hub/Editor/2022.3.0f1 bundle exec rake plugin:export
```

---

## Build System

All build tasks are defined in the `Rakefile` and invoked with `bundle exec rake`. List available tasks:

```
bundle exec rake -T
```

### Common tasks

| Task | Description |
|---|---|
| `rake plugin:export` | Full clean build — produces `Bugsnag.unitypackage` |
| `rake plugin:quick_export` | Incremental build (skips clean step) |
| `rake plugin:release` | Tags the release and triggers the CI release build |
| `rake plugin:package` | Packages for UPM distribution |
| `rake example:build:all` | Builds the example app for all platforms |
| `rake test:android:build` | Builds the Android E2E fixture APK |
| `rake test:ios:build` | Builds the iOS E2E fixture IPA |

### Produced artefacts

- `Bugsnag.unitypackage` — standard Unity package for manual import
- `Bugsnag-with-android-64bit.unitypackage` — variant bundling 64-bit Android libs
- `features/fixtures/maze_runner/mazerunner_<version>.apk` (Android)
- `features/fixtures/maze_runner/mazerunner_<version>.ipa` (iOS)
- `features/fixtures/maze_runner/build/MacOS/Mazerunner.app` (macOS)

### Android bridge (`bugsnag-android-unity/`)

This sub-project is a Gradle library (compile SDK 34, min SDK 21) that bridges Unity C# calls to the bugsnag-android Java SDK. If you change Java code here, rebuild with:

```
cd bugsnag-android-unity && ./gradlew assembleRelease
```

---

## Running Tests

### Unit tests

Unit tests live in `Bugsnag/Assets/Tests/` and run as Unity EditMode tests. They cover configuration validation, session tracking, stack frame parsing, log deduplication, and API surface checks.

Run them from within the Unity Editor (**Window › Test Runner › EditMode**) or via the relevant `rake test:unit:*` task.

### End-to-end tests (Maze Runner)

E2E tests use [Maze Runner](https://github.com/bugsnag/maze-runner), a Cucumber/Gherkin framework that drives a real built app and intercepts its HTTP traffic.

#### 1. Build the plugin

```
bundle exec rake plugin:export
```

#### 2. Build the target fixture

```
# Android
UNITY_VERSION=2022.3.0f1 rake test:android:build

# iOS
UNITY_VERSION=2022.3.0f1 rake test:ios:build

# macOS
UNITY_VERSION=2022.3.0f1 ./features/scripts/build_maze_runner.sh macos
```

#### 3. Run the tests

**macOS desktop:**

```
bundle exec maze-runner --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app --os=macos
```

**Android (BrowserStack — Bugsnag employees only):**

```
bundle exec maze-runner \
  --app=features/fixtures/maze_runner/mazerunner_2022.3.0f1.apk \
  --farm=bs \
  --device=ANDROID_9_0
```

**iOS (BrowserStack — Bugsnag employees only):**

```
bundle exec maze-runner \
  --app=features/fixtures/maze_runner/mazerunner_2022.3.0f1.ipa \
  --farm=bs \
  --device=IOS_14
```

> Mobile E2E tests require BrowserStack credentials (`BROWSER_STACK_USERNAME`, `BROWSER_STACK_ACCESS_KEY`, `MAZE_BS_LOCAL`). These are only available to Bugsnag employees.

**WebGL (requires Chrome + chromedriver):**

```
bundle exec maze-runner --farm=local --browser=chrome
```

### Feature files

Feature files are organised by platform under `features/`:

```
features/
├── android/          # android_config, android_callbacks, android_jvm_errors, android_ndk_errors …
├── ios/              # ios_config, ios_native_errors, ios_callbacks …
├── macos/
├── webgl/
└── csharp/           # Cross-platform C# managed exceptions
```

Step definitions are implemented in Ruby in `features/steps/unity_steps.rb`.

---

## Native Platform Bridges

Each platform bridge lives under `Bugsnag/Assets/Bugsnag/Runtime/Native/<Platform>/` and implements `INativeClient`.

| Platform | Directory | Mechanism |
|---|---|---|
| Android | `Native/Android/` | C# → JNI → `bugsnag-android-unity` AAR → bugsnag-android SDK |
| iOS | `Native/iOS/` + `Native/Cocoa/` | C# → P/Invoke → Objective-C → bugsnag-cocoa SDK |
| macOS | `Native/MacOS/` + `Native/Cocoa/` | C# → P/Invoke → Objective-C → bugsnag-cocoa SDK |
| Windows | `Native/Windows/` | Pure C# implementation |
| WebGL | `Native/Fallback/` | Pure C# (no native crash capture) |

The Android and iOS/macOS native SDKs are included as Git submodules (`bugsnag-android/`, `bugsnag-cocoa/`) and built as part of the fixture build process.

### Adding a new native capability

1. Add the method to `INativeClient` (`Runtime/INativeClient.cs`).
2. Implement it in each platform class under `Native/`.
3. Add a no-op in `Native/Fallback/` so unsupported platforms compile cleanly.
4. Expose the capability from `Client.cs` (and optionally from the `Bugsnag` static façade).

---

## Key Source Files and Concepts

| File | What it does |
|---|---|
| `Runtime/Bugsnag.cs` | Static public API — every user-facing method is here |
| `Runtime/Client.cs` | Core logic: log capture, event construction, callbacks, sessions |
| `Runtime/Configuration.cs` | All SDK configuration options and feature flags |
| `Runtime/Delivery.cs` | HTTP delivery, payload size truncation |
| `Runtime/SessionTracker.cs` | Auto and manual session lifecycle |
| `Runtime/PayloadManager.cs` | Queuing and retry for offline events |
| `Runtime/BugsnagAutoInit.cs` | Scriptable Object that auto-starts Bugsnag from the Editor |
| `Runtime/Payload/Event.cs` | Top-level error event model |
| `Runtime/Payload/ErrorBuilder.cs` | Builds `Error` objects from C# exceptions and stack traces |
| `Runtime/SimpleJson.cs` | Bundled JSON serialiser (no external dependency) |
| `Editor/BugsnagEditor.cs` | Inspector UI for the Bugsnag settings asset |
| `Editor/BuildPreprocessor.cs` | Runs before Unity builds (injects API key, configures native projects) |
| `Editor/SymbolUpload/` | Automatic dSYM / symbol upload via the Bugsnag CLI |

### Payload size management

`Delivery.cs` enforces a 1 MB payload limit. Metadata strings and breadcrumbs are iteratively truncated when a payload exceeds this limit before it is sent.

### Automatic initialisation

`BugsnagAutoInit` is a `ScriptableObject` that is loaded during the Unity startup sequence. When enabled in the Editor settings dialog, it calls `Bugsnag.Start()` before the first scene loads, so no code changes are required in the game.

### IL2CPP compatibility

`Il2cppUtils.cs` provides helpers that work around IL2CPP limitations (e.g. reflection restrictions). Use these utilities when writing code that must work in both Mono and IL2CPP scripting backends.

---

## CI/CD

The project uses [BuildKite](https://buildkite.com) for continuous integration. Pipeline definitions are in `.buildkite/`.

| File | Purpose |
|---|---|
| `pipeline.basic.yml` | Default pipeline: EditMode unit tests |
| `pipeline.full.yml` | Extended pipeline: unit + all platform E2E tests |
| `unity.2020.yml` — `unity.6000.yml` | Per-Unity-version E2E test matrices |
| `pipeline_trigger_*.sh` | Dynamic pipeline selection logic |

Pull requests run the basic pipeline. Commits to `master` / `next` and release tags run the full multi-version matrix.

GitHub Actions workflows are defined in `.github/workflows/` and handle tasks such as UPM package import verification.

---

## Release Process

A full release checklist is in [CONTRIBUTING.md](CONTRIBUTING.md#release-checklist). The high-level steps are:

1. Merge all changes from `master` into `next`.
2. Bump the version in `CHANGELOG.md` and `Bugsnag/Assets/Bugsnag/Runtime/AssemblyInfo.cs`.
3. Open a PR from `next` → `master` and merge.
4. Run `bundle exec rake plugin:release` to tag and trigger the CI release build.
5. CI produces the `.unitypackage` artefacts and creates a draft GitHub Release.
6. Publish the draft release after reviewing the artefacts and copying the changelog entry.
7. Run `bundle exec rake plugin:package` to publish the UPM release.

### Version locations

When bumping the version, update:

- `CHANGELOG.md` — add a new section header.
- `Bugsnag/Assets/Bugsnag/Runtime/AssemblyInfo.cs` — `AssemblyVersion` and `AssemblyFileVersion` attributes.
- The UPM `package.json` is updated automatically by the `plugin:package` Rake task.
