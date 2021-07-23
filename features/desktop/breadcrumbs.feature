Feature: Leaving breadcrumbs to attach to reports

    Scenario: Attaching a message breadcrumb directly
        When I run the game in the "MessageBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "manual" breadcrumb named "Initialize bumpers"

    Scenario: Checking for the bugsnag loaded state breadcrumb
        When I run the game in the "MessageBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
        And the event has a "state" breadcrumb named "Bugsnag loaded"

    Scenario: Attaching a low-level log message as a breadcrumb
        When I run the game in the "DebugLogBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invalid runtime"
        And the event has a "log" breadcrumb named "Warning"
        And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
        And the event "breadcrumbs.1.metaData.message" equals "Failed to validate credentials"

    Scenario: Attaching a complex breadcrumb to a report
        When I run the game in the "ComplexBreadcrumbNotify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "navigation" breadcrumb named "Reload"
        And the event "breadcrumbs.1.metaData.preload" equals "launch"

    Scenario: Attaching an error report breadcrumb
        When I run the game in the "DoubleNotify" state
        And I wait to receive 2 errors
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "Rollback failed"
        And I discard the oldest error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "Invalid runtime"
        And the event "breadcrumbs.1.type" equals "error"
        And the event "breadcrumbs.1.name" equals "Exception"
        And the event "breadcrumbs.1.metaData.message" equals "Rollback failed"

    Scenario: Disabling Breadcrumbs
        When I run the game in the "DisableBreadcrumbs" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the event "breadcrumbs.0" is null

    Scenario: Only enable Log Breadcrumbs
        When I run the game in the "OnlyLogBreadcrumbs" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the event "breadcrumbs.0.type" equals "log"

    Scenario: Setting max breadcrumbs
        When I run the game in the "MaxBreadcrumbs" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the error payload field "events.0.breadcrumbs" is an array with 5 elements
        And the event "breadcrumbs.0.name" equals "Crumb 2"
        And the event "breadcrumbs.1.name" equals "Crumb 3"
        And the event "breadcrumbs.2.name" equals "Crumb 4"
        And the event "breadcrumbs.3.name" equals "Crumb 5"
        And the event "breadcrumbs.4.name" equals "Log"


