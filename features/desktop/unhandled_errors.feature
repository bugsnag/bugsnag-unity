Feature: Reporting unhandled events

  @skip_unity_2018
  Scenario: Reporting an inner exception
    When I run the game in the "InnerException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the event "exceptions.0.message" equals "Outer"
    And the event "exceptions.1.message" equals "Inner"
    And the event "exceptions.2" is null

  Scenario: Reporting an uncaught exception
    When I run the game in the "UncaughtException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "ExecutionEngineException"
    And the exception "message" equals "Promise Rejection"
    And the event "unhandled" is false
    And the event "device.runtimeVersions.unity" is not null
    And the event "device.runtimeVersions.unityScriptingBackend" is not null
    And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
    And the event "device.runtimeVersions.dotnetApiCompatibility" is not null
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoUnhandledException(Int64 counter) | Main.DoUnhandledException(System.Int64 counter) | Main.DoUnhandledException(long counter) |
      | Main.RunScenario(System.String scenario) | Main.RunScenario(string scenario)               |                                         |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) | |

  Scenario: Reporting an uncaught exception in an async method
    When I run the game in the "AsyncException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "AsyncException"
    And the event "unhandled" is false
    And the event "device.runtimeVersions.unity" is not null
    And the event "device.runtimeVersions.unityScriptingBackend" is not null
    And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
    And the event "device.runtimeVersions.dotnetApiCompatibility" is not null
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main+<DoAsyncTest>d__51.MoveNext() |
      | --- End of stack trace from previous location where exception was thrown --- |
      | System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw() |

  Scenario: Session is present in exception called directly after start
    When I run the game in the "ExceptionWithSessionAfterStart" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "ExceptionWithSessionAfterStart"
    And the event "session" is not null

  @windows_only
  Scenario: Windows device and app data
    When I run the game in the "UncaughtException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "ExecutionEngineException"
    And the exception "message" equals "Promise Rejection"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.DoUnhandledException(Int64 counter) | Main.DoUnhandledException(System.Int64 counter) | Main.DoUnhandledException(long counter) |
      | Main.RunScenario(System.String scenario) | Main.RunScenario(string scenario)               |                                         |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) | UnityEngine.SetupCoroutine.InvokeMoveNext(System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) | |

    # Device metadata
    And the event "device.freeDisk" is greater than 0
    And the event "device.freeMemory" is greater than 0
    And the event "device.id" is not null
    And the event "device.locale" is not null
    And the event "device.manufacturer" is not null
    And the event "device.model" is not null
    And the event "device.osName" is not null
    And the event "device.osVersion" is not null
    And the event "device.runtimeVersions" is not null
    And the event "device.time" is a timestamp
    And the event "device.totalMemory" is greater than 0

    # Auto metadata
    And the event "metaData.device.screenDensity" is not null
    And the event "metaData.device.screenResolution" is not null
    And the event "metaData.device.osLanguage" equals "English"
    And the event "metaData.device.graphicsDeviceVersion" is not null
    And the event "metaData.device.graphicsMemorySize" is not null
    And the event "metaData.device.processorType" is not null

    # App metadata
    And the event "app.duration" is greater than 0
    And the event "app.durationInForeground" is not null
    And the event "app.inForeground" is not null
    And the event "app.isLaunching" is not null
    And the event "app.lowMemory" is not null
    And the event "app.releaseStage" is not null
    And the event "app.type" equals "Windows"
    And the event "app.version" is not null

    # Auto app data
    And the event "metaData.app.companyName" equals "bugsnag"
    And the event "metaData.app.name" equals "Mazerunner"
    And the event "metaData.app.buildno" is not null

  Scenario: Forcing uncaught exceptions to be unhandled
    When I run the game in the "UncaughtExceptionAsUnhandled" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "ExecutionEngineException"
    And the exception "message" equals "Invariant state failure"
    And the event "unhandled" is true
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.RunScenario(System.String scenario) | Main.RunScenario(string scenario) |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |

  Scenario: Reporting an assertion failure
    When I run the game in the "AssertionFailure" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "IndexOutOfRangeException"
    And the event "exceptions.0.message" matches one of:
      | Array index is out of range.               |
      | Index was outside the bounds of the array. |
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | Main.MakeAssertionFailure(Int32 counter) | Main.MakeAssertionFailure(System.Int32 counter) | Main.MakeAssertionFailure(int counter) |
      | Main.RunScenario(System.String scenario) | Main.RunScenario(string scenario)               |                                        |
      | UnityEngine.SetupCoroutine.InvokeMoveNext(System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) | |

  @macos_only
  Scenario: Reporting a native crash
    When I run the game in the "NativeCrash" state
    And I run the game in the "(noop)" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the native Unity notifier
    And the exception "errorClass" equals "SIGABRT"
    And the event "unhandled" is true
    And the stack frame methods should match:
      | __pthread_kill       |
      | abort                |
      | crashy_signal_runner |
    And the error payload field "notifier.name" equals "Unity Bugsnag Notifier"
    And custom metadata is included in the event

  Scenario: Encountering a handled event when the current release stage is not in "notify release stages"
    When I run the game in the "UncaughtExceptionOutsideNotifyReleaseStages" state
    Then I should receive no requests

  @macos_only
  Scenario: Encountering a handled event when the current release stage is not in "notify release stages"
    When I run the game in the "NativeCrashOutsideNotifyReleaseStages" state
    And I run the game in the "(noop)" state
    Then I should receive no requests

  Scenario: Reporting an uncaught exception when AutoNotify = false
    When I run the game in the "UncaughtExceptionWithoutAutoNotify" state
    Then I should receive no requests

  Scenario: Discarding An Error Class
    When I run the game in the "DiscardErrorClass" state
    Then I should receive no errors

  @macos_only
  Scenario: Reporting a native crash when AutoNotify = false
    When I run the game in the "NativeCrashWithoutAutoNotify" state
    And I run the game in the "(noop)" state
    Then I should receive no requests

  @macos_only
  Scenario: Reporting a native crash after toggling AutoNotify off then on again
    When I run the game in the "NativeCrashReEnableAutoNotify" state
    And I run the game in the "(noop)" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the native Unity notifier
    And the exception "errorClass" equals "SIGABRT"
    And the event "unhandled" is true
    And the stack frame methods should match:
      | __pthread_kill       |
      | abort                |
      | crashy_signal_runner |
    And the error payload field "notifier.name" equals "Unity Bugsnag Notifier"
    And custom metadata is included in the event

  @skip_webgl
  Scenario: Report exception from background thread
    When I run the game in the "BackgroundThreadCrash" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Background Thread Crash"

  @windows_only
  Scenario: Fallback Launch Duration set to 0
    When I run the game in the "InfLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Launch"
    And the event "app.isLaunching" is true

  @windows_only
  Scenario: Calling Mark Launch Complete
    When I run the game in the "InfLaunchDurationMark" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfLaunchDurationMark"
    And the event "app.isLaunching" is false

  @macos_only
  Scenario: Fallback Launch Duration set to 0
    When I run the game in the "InfLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Launch"
    And the event "app.isLaunching" equals "true"

  @macos_only
  Scenario: Calling Mark Launch Complete
    When I run the game in the "InfLaunchDurationMark" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfLaunchDurationMark"
    And the event "app.isLaunching" equals "false"

  @macos_only
  Scenario: Setting long launch time
    When I run the game in the "LongLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Launch"
    And the event "app.isLaunching" equals "true"

  @macos_only
  Scenario: Setting short launch time
    When I run the game in the "ShortLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Launch"
    And the event "app.isLaunching" equals "false"

  @windows_only
  Scenario: Setting long launch time
    When I run the game in the "LongLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Launch"
    And the event "app.isLaunching" is true
