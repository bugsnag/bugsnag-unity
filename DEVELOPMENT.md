# Bugsnag Unity SDK — Developer Overview

## Repository

[https://github.com/bugsnag/bugsnag-unity](https://github.com/bugsnag/bugsnag-unity)

---

## Architectural Overview

The SDK is structured as a thin C# layer on top of per-platform native notifiers, all exposed through a single static façade.

```
User code
   │
   ▼
Bugsnag              (static façade — Bugsnag.cs)
   │  lock-protected singleton
   ▼
Client               (core C# logic — Client.cs)
   │
   ├── Configuration            all options & feature flags
   ├── IBreadcrumbs             breadcrumb ring buffer
   ├── ISessionTracker          session lifecycle
   ├── PayloadManager           offline event queue
   ├── Delivery                 HTTP send + truncation
   │
   └── INativeClient  ──────────────────────────────────────┐
         ├── Native/Android/   C# → JNI → bugsnag-android   │
         ├── Native/Cocoa/     C# → P/Invoke → bugsnag-cocoa │
         ├── Native/iOS/       iOS bridge entry              │
         ├── Native/MacOS/     macOS bridge entry            │
         ├── Native/Windows/   pure C# implementation        │
         └── Native/Fallback/  no-op (WebGL, unsupported)   ─┘
```

### Key source files

| File | Role |
|---|---|
| `Runtime/Bugsnag.cs` | Public static API |
| `Runtime/Client.cs` | Core logic: log capture, event construction, callbacks, sessions |
| `Runtime/Configuration.cs` | All configuration options |
| `Runtime/Delivery.cs` | HTTP delivery and payload truncation |
| `Runtime/SessionTracker.cs` | Auto/manual session lifecycle |
| `Runtime/PayloadManager.cs` | Event/session queue and retry |
| `Runtime/BugsnagAutoInit.cs` | `ScriptableObject` for auto-start from the Editor |
| `Runtime/Payload/Event.cs` | Top-level error event model |
| `Runtime/Payload/ErrorBuilder.cs` | Constructs `Error` objects from C# exceptions |
| `Runtime/SimpleJson.cs` | Bundled JSON serialiser (no external dependency) |
| `Editor/BuildPreprocessor.cs` | Pre-build hook; injects API key and configures native projects |
| `Editor/SymbolUpload/` | Automatic symbol upload via the Bugsnag CLI |

### Threading model

- Unity log capture, event construction, callbacks, and session management all run on the main thread.
- Payload delivery runs on a background thread (via Unity coroutines and `UnityWebRequest`).
- Network connectivity checks run on a background thread to avoid blocking.
- Shared state is protected by per-object locks: `_clientLock`, `CallbackLock`, `_onErrorLock`, etc.

---

## Sub-systems

### Native bridges

Each platform bridge lives under `Bugsnag/Assets/Bugsnag/Runtime/Native/<Platform>/` and implements `INativeClient`. The correct implementation is selected at compile time via platform-specific assembly definitions and `#if` guards.

| Platform | Directory | Mechanism |
|---|---|---|
| Android | `Native/Android/` | C# → JNI → `bugsnag-android-unity` AAR → bugsnag-android |
| iOS | `Native/iOS/` + `Native/Cocoa/` | C# → P/Invoke → Objective-C → bugsnag-cocoa |
| macOS | `Native/MacOS/` + `Native/Cocoa/` | C# → P/Invoke → Objective-C → bugsnag-cocoa |
| Windows | `Native/Windows/` | Pure C# |
| WebGL / other | `Native/Fallback/` | Pure C# (no native crash capture) |

`bugsnag-android` and `bugsnag-cocoa` are included as Git submodules. Their pre-built outputs (AAR / `.a` / `.dylib`) are committed under `android-libs/` and consumed directly.

The `bugsnag-android-unity/` sub-project is a Gradle library (compile SDK 34, min SDK 21) that bridges Unity C# calls to bugsnag-android. Rebuild after Java changes with:

```
cd bugsnag-android-unity && ./gradlew assembleRelease
```

To add a new native capability: add the method to `INativeClient`, implement it in every platform directory, add a no-op in `Native/Fallback/`, then expose it from `Client.cs` and `Bugsnag.cs`.

### IL2CPP compatibility

`Il2cppUtils.cs` provides helpers that work around IL2CPP reflection restrictions. Use these utilities for any code that must run under both Mono and IL2CPP scripting backends.

### Editor tooling

`Editor/BugsnagEditor.cs` provides the Inspector UI for the `BugsnagSettingsObject` asset.
`Editor/BuildPreprocessor.cs` implements `IPreprocessBuildWithReport` and runs before every build to inject the API key into native project files.

---

## Initialization

**`Bugsnag.Start()`** is the entry point (`Bugsnag.cs`). It:
1. Acquires `_clientLock` to guarantee single initialisation.
2. Clones the supplied `Configuration` (so later mutation does not affect a running SDK).
3. Configures endpoints on the clone.
4. Constructs the platform-specific `NativeClient`.
5. Wraps it in a `Client` instance and assigns it to the static `InternalClient`.
6. A second call to `Start()` logs a warning and is otherwise ignored.

**Automatic initialisation via `BugsnagAutoInit`:**
`BugsnagAutoInit` is a `ScriptableObject` stored under `Resources/`. It is loaded via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, which runs before any scene is loaded. If `StartAutomaticallyAtLaunch` is true in the settings asset, it calls `Bugsnag.Start(config)` automatically — no code change is required in the game. During this initialisation the scripting backend (Mono/IL2CPP), .NET runtime, and API compatibility level are detected and stored on `Configuration`.

**State before `Start()` is called:**
`InternalClient` is `null`. Every public method on `Bugsnag` guards against this with a null check and silently does nothing, so calling SDK methods before `Start()` does not throw.

**Background work at start-up:**
Session tracking begins on the main thread. Cached (offline) events and sessions are re-delivered immediately after start, with network connectivity checks pushed to a background thread to avoid blocking the main thread.

---

## Error Handling

### Unity C# exceptions

`BugsnagLogHandler` (inner class of `Client`) wraps Unity's existing `ILogHandler`. It is installed via `SetupAdvancedExceptionInterceptor()` on Unity 2019+. When `LogException()` is called, the handler:

1. Checks `Configuration.AutoDetectErrors` and `NotifyLogLevel`.
2. Applies duplicate suppression (`UniqueLogThrottle`) and per-type rate limiting (`MaximumLogTypeCounter`).
3. Marks the event as handled or unhandled depending on `ReportExceptionLogsAsHandled`.
4. Forwards to the original handler to preserve console output.

For Unity < 2019, `Application.logMessageReceivedThreaded` and `Application.logMessageReceived` events are used instead.

### Unity log messages

`EnabledErrorTypes.UnityLog` controls whether non-exception log messages (errors, warnings) are captured. These are captured via the same `Application.logMessageReceived` path and created as handled events.

### Android JVM exceptions

The Android native bridge detects `"AndroidJavaException: "` prefixes in log messages and routes them through the normal C# error path. The bugsnag-android SDK also independently captures JVM exceptions from the Java layer via its own uncaught exception handler.

### Android NDK crashes

bugsnag-android handles NDK crash capture natively. The signal handler and crash report are entirely within the bugsnag-android layer; the Unity C# layer has no involvement at crash time.

### iOS / macOS native crashes

bugsnag-cocoa captures Objective-C exceptions and C signal-based crashes. These are handled entirely in the native layer and reported independently of the Unity C# layer.

### App hangs (iOS / Android)

Configured via `AppHangThresholdMillis` (default 0 = disabled; minimum meaningful value is 250 ms) and `EnabledErrorTypes.AppHangs`. Detection is delegated to the native notifier; no C# implementation is involved.

### ANRs (Android)

Controlled by `EnabledErrorTypes.ANRs` (default true). Detected and reported by bugsnag-android.

---

## Stacktrace Capture

### C# managed stack traces

`ErrorBuilder` constructs `Error` objects from C# `System.Exception` instances. Inner exceptions are flattened via `ErrorBuilder.EnumerateFrom()`. Each exception's `StackTrace` string (or a `StackFrame[]` from `System.Diagnostics`) is passed to `PayloadStackTrace`, which parses it into `StackTraceLine` objects.

`StackTraceLine` uses regular expressions to support multiple formats:
- Standard Unity managed format
- Android Java format (`at com.example.Foo.bar(Foo.java:14)`)
- Mixed managed/native wrappers

The `inProject` flag on each frame is set by matching the declaring type's assembly against `Configuration.ProjectPackages`.

### IL2CPP native stack traces

On IL2CPP builds (Unity 2021.3+), `Il2cppUtils` uses P/Invoke to call `il2cpp_native_stack_trace()` to obtain native frame addresses, image UUIDs, and image names. These are stored as `StackTraceLine.FrameAddress` and `StackTraceLine.LoadAddress`, which are later used for server-side symbolication.

### Thread traces

`SendThreads` (`ThreadSendPolicy`) controls whether thread information is included:
- `UnhandledOnly` (default): thread info included only for unhandled events.
- `All`: thread info included for all events.

The maximum number of threads reported per event is `MaxReportedThreads` (default 200).

---

## Handled Events

### Notify overloads

All of the following are on `Bugsnag` (public façade) and delegate to `Client`:

```csharp
Bugsnag.Notify(Exception exception)
Bugsnag.Notify(Exception exception, Func<IEvent, bool> callback)
Bugsnag.Notify(Exception exception, Severity severity, Func<IEvent, bool> callback)
Bugsnag.Notify(string name, string message, string stackTrace, Func<IEvent, bool> callback)
Bugsnag.Notify(Exception exception, string stacktrace, Func<IEvent, bool> callback)
```

### Execution flow

1. `ErrorBuilder.EnumerateFrom()` flattens inner exceptions.
2. If not on the main thread, the call is marshalled to the main thread via `MainThreadDispatchBehaviour`.
3. On the main thread: `AppWithState` and `DeviceWithState` are built, metadata is merged from the native layer and stored metadata, feature flags are snapshotted, and an `Event` object is constructed.
4. `OnError` callbacks are invoked (under lock). If any returns `false`, the event is discarded.
5. The per-call `callback` argument (if supplied) is invoked.
6. A `Report` is created and added to `PayloadManager`, which triggers `Delivery`.
7. An error breadcrumb is left automatically (if enabled).
8. The handled exception count on the current session is incremented.

### Early-exit conditions

- `AutoDetectErrors` is false (for auto-captured errors)
- Release stage is not in `EnabledReleaseStages`
- Error class matches a pattern in `DiscardClasses`
- Endpoints are not configured
- An `OnError` callback returns `false`

---

## Callbacks

### Available callbacks

| Callback | Delegate type | When invoked |
|---|---|---|
| `AddOnError` / `RemoveOnError` | `Func<IEvent, bool>` | When any error/exception occurs, before delivery |
| `AddOnSendError` / `RemoveOnSendError` | `Func<IEvent, bool>` | Immediately before payload is sent over the network |
| `AddOnSession` / `RemoveOnSession` | `Func<ISession, bool>` | When a session is created |

### Storage and invocation

Each callback list is stored in `Configuration` as a `List<Func<…>>` with a corresponding lock object (`_onErrorLock`, `_onSendErrorLock`, `_onSessionLock`). Before iterating, the list is copied with `.ToList()` to prevent concurrent-modification issues.

- **OnError** — invoked in `Client.NotifyOnMainThread()`. Returning `false` discards the event.
- **OnSendError** — invoked in `Delivery.Deliver()` as the final opportunity to modify or discard an event before it leaves the device. Returning `false` discards the event.
- **OnSession** — invoked in `SessionTracker.StartManagedSession()`. Returning `false` discards the session.

Exceptions thrown inside callbacks are caught silently to prevent callback errors from disrupting the SDK.

---

## Breadcrumbs

### Storage

Breadcrumbs are stored in a `LinkedList<Breadcrumb>` with a `readonly object _lock` for thread safety (in `Native/Fallback/Breadcrumbs.cs`). When the list reaches `Configuration.MaximumBreadcrumbs`, the oldest entry (head of the list) is removed before appending the new one, giving FIFO circular buffer behaviour. `Retrieve()` returns a `.ToList()` copy to prevent external mutation.

### Automatic breadcrumb types

| Type | Trigger |
|---|---|
| `State` | SDK initialisation ("Bugsnag loaded") |
| `Navigation` | Unity scene loads (`SceneManager.sceneLoaded`) |
| `Log` | Unity log messages at or above `BreadcrumbLogLevel` |
| `Error` | Each error reported (via `Breadcrumb.FromReport()`) |
| `Request` | Network requests tracked via `LeaveNetworkBreadcrumb()` / `BugsnagUnityWebRequest` |

`EnabledBreadcrumbTypes` (default `null` = all enabled) controls which automatic types are captured.

### Limits

`MaximumBreadcrumbs` defaults to 100 and can be set to 0–500. Setting it to 0 effectively disables breadcrumb capture.

### Caveats

On Android, iOS, and macOS, breadcrumbs are also forwarded to the native notifier so they are available even if the app crashes before the C# layer can deliver them.

---

## Metadata

### Storage

`Configuration` holds a `Metadata` instance (a `Dictionary<string, object>` where each value is itself a `Dictionary<string, object>` keyed by section name). `Client` copies the reference into `_storedMetadata` at start-up. All `AddMetadata` / `ClearMetadata` calls update `_storedMetadata` and simultaneously propagate to the native layer via `INativeClient`.

### Defensive copies

- Breadcrumb metadata dictionaries are shallow-copied via `.ToDictionary()` when stored.
- When building an event, `_storedMetadata` is merged into the event by reference (no deep copy). This is by design for performance. Mutating metadata after `Notify()` returns but before delivery has completed can affect the payload in flight; avoid mutating metadata from a different thread concurrently with a `Notify()` call.

### Automatically collected data

The native layer (`AutomaticDataCollector`) adds device info (OS version, model, free memory, locale, etc.) and app info (bundle ID, version, build, release stage) to every event. These are not stored in the C# `Metadata` dictionary; they appear in the top-level `device` and `app` payload objects.

### Key redaction

`RedactedKeys` is a `List<Regex>` (default: `.*password.*`). Any metadata key whose name matches a pattern has its value replaced with `[REDACTED]` in the serialised payload.

---

## Feature Flags

### Storage

Feature flags are stored in an `OrderedDictionary` (`Configuration.FeatureFlags`) to preserve insertion order. Keys are flag names (strings); values are variants (strings or `null`).

### Defensive copies

When building an event (`Client.NotifyOnMainThread()`), the `OrderedDictionary` is iterated and copied into a new `OrderedDictionary` so that the event snapshot is isolated from subsequent flag changes.

### Serialisation

Feature flags are included in every event as a JSON array. Each entry has `name` and (optionally) `variant` fields. The array is built from the snapshot taken at event-build time.

---

## Launch Crashes

### Mechanism

`LaunchDurationMillis` (default 5000 ms) defines the window after app start during which a crash is classified as a launch crash. The `app.isLaunching` field in the event payload is set to `true` for events that occur within this window.

`SendLaunchCrashesSynchronously` (default `true`) causes the SDK to block the calling thread until any launch-period crash has been delivered, ensuring it is not lost if the app is about to terminate. Call `Bugsnag.MarkLaunchCompleted()` to end the launch period early.

Setting `LaunchDurationMillis = 0` disables the launch crash classification entirely.

---

## Network Errors

### Mechanism

`BugsnagUnityWebRequest` is a wrapper around `UnityWebRequest` that can be used in place of the standard class. When a request completes, `Bugsnag.LeaveBreadcrumb(request, duration)` is called, leaving a `Request` breadcrumb containing the URL, HTTP method, status code, and duration.

Network breadcrumb capture is controlled by `EnabledBreadcrumbTypes` and must be opted-in to by using `BugsnagUnityWebRequest` or calling `Bugsnag.LeaveBreadcrumb(UnityWebRequest, TimeSpan?)` manually. There is no automatic network request interception.

---

## Session Tracking

### Session start triggers

- **Automatic:** If `AutoTrackSessions = true`, a session starts when `Bugsnag.Start()` is called. When the app returns to the foreground after being in the background for more than `AutoCaptureSessionThresholdSeconds` (30 seconds, hardcoded in `Client.cs`), a new session is started automatically.
- **Manual:** `Bugsnag.StartSession()` / `PauseSession()` / `ResumeSession()`.

### Storage

On Android, iOS, and macOS, session management is delegated to the native notifier, which handles persistence. On Fallback platforms (Windows, WebGL), `SessionTracker` manages the current session entirely in C#. The active `Session` object stores: a UUID, start time, `App`, `Device`, `User`, and handled/unhandled exception counts. `SessionTracker.CurrentSession` returns a copy via `Session.Copy()` to prevent external mutation.

Session payloads are written to disk under `Application.persistentDataPath/Bugsnag/Sessions/` as `{uuid}.session` files and re-delivered on the next app launch if a previous delivery failed.

---

## Configuration Options

The following options are particularly noteworthy for platform-specific or non-obvious behaviour:

| Option | Default | Notes |
|---|---|---|
| `ReportExceptionLogsAsHandled` | `true` | Exceptions reported via `Debug.LogException()` are marked as **handled**. Set to `false` to treat them as unhandled. |
| `SendLaunchCrashesSynchronously` | `true` | Blocks the main thread until any crash that occurred within `LaunchDurationMillis` has been delivered. |
| `LaunchDurationMillis` | `5000` | Set to `0` to disable launch crash classification. |
| `AppHangThresholdMillis` | `0` | Disabled by default. Minimum is 250 ms. Delegated to the native layer. |
| `SendThreads` | `UnhandledOnly` | `All` sends thread state with every event; can increase payload size significantly. |
| `MaxReportedThreads` | `200` | Caps threads per event to limit payload size. |
| `MaximumBreadcrumbs` | `100` | Valid range 0–500. |
| `BreadcrumbLogLevel` | `LogType.Log` | Minimum Unity log level that triggers an automatic breadcrumb. |
| `RedactedKeys` | `.*password.*` | List of `Regex` patterns; matching metadata keys are replaced with `[REDACTED]`. |
| `DiscardClasses` | _(empty)_ | List of `Regex` patterns; matching error class names are silently discarded. |
| `ProjectPackages` | `null` | Package name prefixes used to mark stack frames as `inProject`. Important for Android frame classification. |
| `MaximumLogsTimePeriod` | `1 s` | Rate-limiting window for log capture. |
| `MaximumTypePerTimePeriod` | See note | A `Dictionary<LogType, int>` capping how many logs of each type are reported per `MaximumLogsTimePeriod`. Defaults: Assert=5, Error=5, Exception=20, Log=5, Warning=5. |
| `SecondsPerUniqueLog` | `5 s` | Duplicate-detection window; identical log messages within this window are suppressed. |
| `PersistUser` | `true` | User is persisted across sessions to disk. |
| `GenerateAnonymousId` | `true` | A UUID device ID is generated and cached if no user ID is set. |
| `SwitchCacheType` | `R` | Nintendo Switch only: `R` (NAND) or `I` (SD card). |
| `Telemetry` | _(all)_ | Controls whether internal SDK errors are reported to Bugsnag. |

---

## Payload Serialisation

The `IPayload` interface is implemented by `Report` (error events) and `SessionReport` (sessions). Each implementation provides `GetSerialisablePayload()`, returning a `Dictionary<string, object>` that maps directly to the Bugsnag event or session JSON structure.

Serialisation is performed by `SimpleJson` — a bundled, dependency-free JSON serialiser. This avoids relying on Unity's `JsonUtility` (which cannot handle arbitrary dictionaries) and external packages (which may not be available on all platforms, including WebGL). `SimpleJson.SerializeObject()` writes to a `TextWriter` / `MemoryStream` and returns a UTF-8 byte array.

Event payloads are wrapped in an `{ "events": [ … ] }` envelope even though only one event is sent per request. Session payloads are sent directly. Request headers include:
- `Bugsnag-Api-Key`
- `Bugsnag-Payload-Version` (`"4.0"` for events, `"1.0"` for sessions)
- `Bugsnag-Sent-At` (ISO 8601 UTC)
- `Bugsnag-Integrity` (SHA-1 hash of the body)

---

## Delivery and Persistence

### Delivery flow

1. `Client.Send(payload)` calls `Delivery.Deliver(payload)`.
2. `OnSendError` callbacks are invoked; returning `false` cancels delivery.
3. A coroutine (`PushToServer`) is enqueued on the main thread.
4. `INativeClient.ShouldAttemptDelivery()` is checked on a background thread (connectivity check).
5. The payload is serialised to JSON bytes. If the result exceeds 1 MB, truncation is applied (see below).
6. A `UnityWebRequest` POST is sent to the configured endpoint.
7. The response determines the next action.

### Retry and persistence

| HTTP status | Action |
|---|---|
| 200, 202 | Success — remove from pending queue and cache |
| 0, 408, 429, 500+ | Transient failure — write to disk for later retry |
| Any other | Permanent failure — discard |

Failed payloads are written to `Application.persistentDataPath/Bugsnag/Events/` (`.event` files) or `.../Sessions/` (`.session` files). A maximum of 32 events and 128 sessions are retained on disk. Files older than 60 days are automatically discarded.

On every app launch (and when the app returns to the foreground), `Delivery.StartDeliveringCachedPayloads()` iterates the cached files and re-attempts delivery.

### Payload truncation (> 1 MB)

1. Truncate all string values in metadata.
2. If still over limit: remove the oldest breadcrumbs one by one.
3. On WebGL: clear all breadcrumbs, then remove the `user` metadata section.

---

## Symbolication

### Android

NDK native crashes contain raw stack addresses. Symbol upload is triggered automatically after an Android build if `EditorUserBuildSettings.androidCreateSymbols` is enabled. The `BugsnagSymbolUploader` editor post-build hook (`callbackOrder = 1`) calls the Bugsnag CLI:

```
bugsnag-cli upload unity-android \
  --api-key=<key> \
  --app-version=<version> \
  --version-code=<versionCode> \
  <buildOutputPath>
```

This uploads `.so` files (NDK native libraries) and the IL2CPP symbol mapping file (Unity 2021.1+; the `--no-upload-il2cpp-mapping` flag is used for older versions).

### iOS and macOS

A shell script build phase named **"BugSnag dSYM Upload"** is injected into the generated Xcode project by `BuildPreprocessor`. It runs during the Xcode "install" action:

```
bugsnag-cli upload unity-ios --dsym-path="$DWARF_DSYM_FOLDER_PATH"
```

This uploads `.dSYM` bundles produced by Xcode.

### Bugsnag CLI

The CLI tool (version 3.3.1) is downloaded automatically from GitHub Releases to `Assets/../bugsnag/bin/bugsnag_cli` for the host platform (macOS x86_64/arm64, Windows x86_64, Linux x86_64). A custom path can be set via `BugsnagSettingsObject.BugsnagCLIExecutablePath`.

### Build identifiers

- **Android:** `versionCode` and `versionName` (from the Player Settings).
- **iOS/macOS:** the dSYM UUID embedded by the linker (extracted by Xcode / `dsymutil`).

---

## Testing Approach

### Unit tests

Seven EditMode test classes in `Bugsnag/Assets/Tests/`:

| Test class | Coverage area |
|---|---|
| `ExceptionTests` | Exception payload construction |
| `ConfigurationTests` | Configuration validation and defaults |
| `SessionTrackerTests` | Session lifecycle |
| `StackFrameParsingTests` | Stack trace regex parsing (Unity, Android Java, mixed formats) |
| `UniqueLogCounterTests` | Duplicate log suppression |
| `MaximumLogTypeCounterTests` | Per-type log rate limiting |
| `OverloadCheck` | Public API surface compatibility |

Run from the Unity Editor (**Window › Test Runner › EditMode**) or via `bundle exec rake`.

### End-to-end tests (Maze Runner)

E2E tests are written in Cucumber/Gherkin (`.feature` files) and executed by [Maze Runner](https://github.com/bugsnag/maze-runner). The test fixture app is a real Unity build that sends requests to a local Maze Runner HTTP server, which validates payloads against the feature file expectations.

Feature files are organised by platform:

```
features/
├── android/          android_config, android_callbacks, android_jvm_errors, android_ndk_errors …
├── ios/              ios_config, ios_native_errors, ios_callbacks …
├── macos/
├── webgl/
├── csharp/           cross-platform C# managed exceptions
└── steps/            unity_steps.rb  (shared Ruby step definitions)
```

### When tests run

- **Pull requests:** Unit tests only (BuildKite `pipeline.basic.yml`).
- **Commits to `master` / `next` and release tags:** Full matrix including E2E (BuildKite `pipeline.full.yml`).
- **GitHub Actions:** UPM package import verification on PRs.

### Test matrix

E2E tests run against Unity 2020.x, 2021.x, 2022.x, and Unity 6 (2024.x). Mobile tests run on real devices via BrowserStack (Android 9+, iOS 14+). macOS and Windows desktop tests run on CI agents directly. WebGL tests use a headless Chrome instance with `chromedriver`.

To build and run E2E tests locally:

```bash
# 1. Build the plugin
bundle exec rake plugin:export

# 2. Build the fixture (example: Android)
UNITY_VERSION=2022.3.0f1 rake test:android:build

# 3. Run (macOS desktop example)
bundle exec maze-runner \
  --app=features/fixtures/maze_runner/build/MacOS/Mazerunner.app \
  --os=macos
```

Mobile E2E tests require BrowserStack credentials (`BROWSER_STACK_USERNAME`, `BROWSER_STACK_ACCESS_KEY`, `MAZE_BS_LOCAL`) which are only available to Bugsnag employees.

---

## Release Process

Full checklist in [CONTRIBUTING.md](CONTRIBUTING.md). Summary:

1. Merge `master` into `next`.
2. Bump version in `CHANGELOG.md` and `Bugsnag/Assets/Bugsnag/Runtime/AssemblyInfo.cs`.
3. Open a PR from `next` → `master` and merge.
4. Run `bundle exec rake plugin:release` to tag and trigger the CI release build.
5. CI produces `.unitypackage` artefacts and opens a draft GitHub Release.
6. Review the draft, paste the changelog entry as release notes, and publish.
7. Run `bundle exec rake plugin:package` to publish the UPM release.

### Release artefacts

| Artefact | Destination |
|---|---|
| `Bugsnag.unitypackage` | GitHub Release assets |
| `Bugsnag-with-android-64bit.unitypackage` | GitHub Release assets |
| UPM package | NPM registry (referenced via git URL) |

### Version bump locations

- `CHANGELOG.md` — new section header.
- `Bugsnag/Assets/Bugsnag/Runtime/AssemblyInfo.cs` — `AssemblyVersion` and `AssemblyFileVersion`.
- `upm/package.json` — updated automatically by `rake plugin:package`.

---

## Related SDKs / Tools

- **[bugsnag-android](https://github.com/bugsnag/bugsnag-android)** — Android native notifier, included as a submodule. Provides JVM and NDK crash capture, ANR detection, and app hang detection on Android.
- **[bugsnag-cocoa](https://github.com/bugsnag/bugsnag-cocoa)** — iOS/macOS native notifier, included as a submodule. Provides Objective-C exception capture, signal-based crash capture, and app hang detection on Apple platforms.
- **[bugsnag-cli](https://github.com/bugsnag/bugsnag-cli)** — CLI tool used to upload symbols for Android (`.so`, IL2CPP mapping) and iOS/macOS (`.dSYM`).
- **[maze-runner](https://github.com/bugsnag/maze-runner)** — End-to-end test harness used by this and all other Bugsnag notifiers.
