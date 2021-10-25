Feature: Reporting unhandled events

    Scenario: Reporting an uncaught exception
        When I run the game in the "UncaughtException" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Promise Rejection"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoUnhandledException(Int64 counter) | Main.DoUnhandledException(System.Int64 counter) |
            | Main.RunScenario(System.String scenario)         | |
            | Main.Start()               | |

    @windows_only
    Scenario: Windows device and app data
        When I run the game in the "UncaughtException" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Promise Rejection"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoUnhandledException(Int64 counter) | Main.DoUnhandledException(System.Int64 counter) |
            | Main.RunScenario(System.String scenario)         | |
            | Main.Start()               | |

        #device metadata
        And the event "device.charging" is not null
        And the event "device.freeDisk" is greater than 0
        And the event "device.freeMemory" is greater than 0
        And the event "device.hostname" is not null
        And the event "device.id" is not null
        And the event "device.locale" is not null
        And the event "device.manufacturer" is not null
        And the event "device.model" is not null
        And the event "device.osName" is not null
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the event "device.screenDensity" is greater than 0
        And the event "device.screenResolution" is not null
        And the event "device.time" is a timestamp
        And the event "device.timezone" is not null
        And the event "device.totalMemory" is greater than 0

        #app metadata
        And the event "app.duration" is greater than 0
        # TODO Pending PLAT-7433
        # And the event "app.durationInForeground" is greater than 0
        And the event "app.inForeground" is not null
        And the event "app.isLaunching" is not null
        And the event "app.lowMemory" is not null
        And the event "app.releaseStage" is not null
        And the event "app.type" equals "Windows"
        And the event "app.version" is not null


    @skip_webgl
    Scenario: Forcing uncaught exceptions to be unhandled
        When I run the game in the "UncaughtExceptionAsUnhandled" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invariant state failure"
        And the event "unhandled" is true
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.RunScenario(System.String scenario)         |
            | Main.Start()               |

    @webgl_only
    Scenario: Forcing uncaught exceptions to be unhandled
        When I run the game in the "UncaughtExceptionAsUnhandled" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invariant state failure"
        And the event "unhandled" is true
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.UncaughtExceptionAsUnhandled() |
            | Main.RunScenario(System.String scenario) |
            | Main.Start()               |

    Scenario: Reporting an assertion failure
        When I run the game in the "AssertionFailure" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "IndexOutOfRangeException"
        And the event "exceptions.0.message" matches one of:
            | Array index is out of range. |
            | Index was outside the bounds of the array. |
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.MakeAssertionFailure(Int32 counter) | Main.MakeAssertionFailure(System.Int32 counter) |
            | Main.RunScenario(System.String scenario)                      | |
            | Main.Start()                            | |

    @macos_only
    Scenario: Reporting a native crash
        When I run the game in the "NativeCrash" state
        And I run the game in the "(noop)" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the native Unity notifier
        And the exception "errorClass" equals "SIGABRT"
        And the event "unhandled" is true
        And the first significant stack frame methods and files should match:
            | __pthread_kill       |
            | abort                |
            | crashy_signal_runner |
        # awaiting fix in PLAT-6495
        # And the payload field "notifier.name" equals "Bugsnag Unity (Cocoa)"
        # And custom metadata is included in the event


    Scenario: Encountering a handled event when the current release stage is not in "notify release stages"
        When I run the game in the "UncaughtExceptionOutsideNotifyReleaseStages" state
        Then I should receive no requests

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
        And the first significant stack frame methods and files should match:
            | __pthread_kill       |
            | abort                |
            | crashy_signal_runner |
        # awaiting fix in PLAT-6495
        # And the payload field "notifier.name" equals "Bugsnag Unity (Cocoa)"
        # And custom metadata is included in the event

    @skip_webgl
    Scenario: Report exception from background thread
        When I run the game in the "BackgroundThreadCrash" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "message" equals "Background Thread Crash"

