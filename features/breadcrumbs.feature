Feature: Leaving breadcrumbs to attach to reports

    Background:
        Given I set environment variable "BUGSNAG_APIKEY" to "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And I build a Unity application

    Scenario: Attaching a message breadcrumb directly
        When I run the game in the "MessageBreadcrumbNotify" state
        Then I should receive a request
        And the request is valid for the error reporting API
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "System.Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "manual" breadcrumb named "Initialize bumpers"

    Scenario: Attaching a low-level log message as a breadcrumb
        When I run the game in the "DebugLogBreadcrumbNotify" state
        Then I should receive a request
        And the request is valid for the error reporting API
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "System.ExecutionEngineException"
        And the exception "message" equals "Invalid runtime"
        And the event has a "log" breadcrumb named "Warning"
        And the payload field "events.0.breadcrumbs.1.metaData.message" equals "Failed to validate credentials"

    Scenario: Attaching a complex breadcrumb to a report
        When I run the game in the "ComplexBreadcrumbNotify" state
        Then I should receive a request
        And the request is valid for the error reporting API
        And the payload field "events" is an array with 1 element
        And the exception "errorClass" equals "System.Exception"
        And the exception "message" equals "Collective failure"
        And the event has a "navigation" breadcrumb named "Reload"
        And the payload field "events.0.breadcrumbs.1.metaData.preload" equals "launch"

    Scenario: Attaching an error report breadcrumb
        When I run the game in the "DoubleNotify" state
        Then I should receive 2 requests
        And request 0 is valid for the error reporting API
        And request 1 is valid for the error reporting API
        And the payload field "events.0.exceptions.0.errorClass" equals "System.Exception" for request 0
        And the payload field "events.0.exceptions.0.message" equals "Rollback failed" for request 0
        And the payload field "events.0.exceptions.0.errorClass" equals "System.ExecutionEngineException" for request 1
        And the payload field "events.0.exceptions.0.message" equals "Invalid runtime" for request 1
        And the payload field "events.0.breadcrumbs.1.type" equals "error" for request 1
        And the payload field "events.0.breadcrumbs.1.name" equals "System.Exception" for request 1
        And the payload field "events.0.breadcrumbs.1.metaData.message" equals "Rollback failed" for request 1
