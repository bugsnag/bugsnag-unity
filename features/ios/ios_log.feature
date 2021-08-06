Feature: iOS smoke tests for log entries

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Calling Bugsnag.Log() with an error

        When I run the "Log error" mobile scenario
        Then I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "UnityLogError"
        And the exception "message" equals "Something went wrong."
        And the event "unhandled" is false
        And the event "severity" equals "warning"
        And the event "severityReason.type" equals "handledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" ends with "UnityEngine.Debug:LogError(Object)"
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.unity.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "iOS"

        # Device data
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" equals "Apple"
        And the event "device.model" is not null
        And the event "device.osName" equals "iOS"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null

        # User
        And the event "user.id" is not null
        And the error payload field "events.0.user.id" equals "mcpacman"

        # Breadcrumbs
        And the event has a "state" breadcrumb named "Bugsnag loaded"

        # Context
        And the event "context" equals "My context"

        # MetaData
        And the event "metaData.device.osLanguage" is not null
        And the event "metaData.app.companyName" equals "bugsnag"
        And the event "metaData.app.name" equals "Mazerunner"

    Scenario: Calling Bugsnag.Log() with an exception

        When I run the "Log caught exception" mobile scenario
        Then I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "IndexOutOfRangeException"
        And the exception "message" equals "Index was outside the bounds of the array."
        And the event "unhandled" is false
        And the event "severity" equals "error"
        And the event "severityReason.type" equals "handledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" ends with "MobileScenarioRunner.LogCaughtException()"
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.unity.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "iOS"

        # Device data
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" equals "Apple"
        And the event "device.model" is not null
        And the event "device.osName" equals "iOS"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null

        # User
        And the event "user.id" is not null
        And the error payload field "events.0.user.id" equals the stored value "device_id"

        # Breadcrumbs
        And the event has a "state" breadcrumb named "Bugsnag loaded"

        # Context
        And the event "context" equals "My context"

        # MetaData
        And the event "metaData.device.osLanguage" is not null
        And the event "metaData.app.companyName" equals "bugsnag"
        And the event "metaData.app.name" equals "Mazerunner"
