Feature: Reporting unhandled events

    Scenario: Reporting an uncaught exception
        When I run the game in the "UncaughtException" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Promise Rejection"
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.DoUnhandledException(Int64 counter) |
            | Main.LoadScenario()         |
            | Main.Update()               |

    Scenario: Forcing uncaught exceptions to be unhandled
        When I run the game in the "UncaughtExceptionAsUnhandled" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invariant state failure"
        And the event "unhandled" is true
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.UncaughtExceptionAsUnhandled() |
            | Main.LoadScenario()         |
            | Main.Update()               |

    Scenario: Reporting an assertion failure
        When I run the game in the "AssertionFailure" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "IndexOutOfRangeException"
        And the exception "message" equals "Array index is out of range."
        And the event "unhandled" is false
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | Main.MakeAssertionFailure(Int32 counter) |
            | Main.LoadScenario()                      |
            | Main.Update()                            |

    Scenario: Reporting a native crash
        When I run the game in the "NativeCrash" state
        And I run the game in the "(noop)" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Bugsnag Unity (Cocoa)"
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "SIGABRT"
        And the event "unhandled" is true
        And custom metadata is included in the event
        And the first significant stack frame methods and files should match:
            | __pthread_kill       |
            | abort                |
            | crashy_signal_runner |
