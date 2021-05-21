Feature: Leaving breadcrumbs to attach to reports

    Scenario: Attaching a message breadcrumb directly
        When I run the game in the "MessageBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "manual" breadcrumb named "Initialize bumpers"

    Scenario: Attaching a low-level log message as a breadcrumb
        When I run the game in the "DebugLogBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invalid runtime"
        And the event has a "log" breadcrumb named "Warning"
        And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
        And the event "breadcrumbs.1.metaData.message" equals "Failed to validate credentials"

    Scenario: Attaching a complex breadcrumb to a report
        When I run the game in the "ComplexBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "navigation" breadcrumb named "Reload"
        And the event "breadcrumbs.1.metaData.preload" equals "launch"

    Scenario: Attaching an error report breadcrumb
        When I run the game in the "DoubleNotify" state
        And I wait to receive 2 errors
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Rollback failed"
        And I discard the oldest error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invalid runtime"
        And the event "breadcrumbs.1.type" equals "error"
        And the event "breadcrumbs.1.name" equals "Exception"
        And the event "breadcrumbs.1.metaData.message" equals "Rollback failed"
