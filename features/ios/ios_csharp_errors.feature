Feature: iOS smoke tests for C# errors

    # TODO: Failing steps commented out pending full implementation of PLAT-6372
    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Uncaught C# exception
        When I run the "throw Exception with breadcrumbs" mobile scenario
        Then I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "Exception"
        And the exception "message" equals "You threw an exception!"
        And the event "unhandled" is false
        And the event "severity" equals "error"
        And the event "severityReason.type" equals "handledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" equals "MobileScenarioRunner.ThrowException()"
        And the event "exceptions.0.stacktrace.0.file" is not null
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 0
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.unity.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "iOS"
        And the event "app.version" equals "1.2.3"
        And the event "app.bundleVersion" equals "1.2.3"
#        And the event "app.versionCode" equals "1"
#        And the error payload field "events.0.app.durationInForeground" is greater than 0
#        And the event "app.inForeground" equals "true"

        # Device data
        And the event "device.jailbroken" is false
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" is not null
        And the event "device.model" is not null
        And the event "device.osName" equals "iOS"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
#        And the event "device.totalMemory" is not null
        And the event "device.freeDisk" is not null
        And the event "device.freeMemory" is not null
#        And the event "device.orientation" equals "portrait"
        And the event "device.time" is not null
#        And the event "device.charging" is not null

        # User
        And the event "user.id" is not null
        And the error payload field "events.0.user.id" equals the stored value "device_id"

        # Breadcrumbs
        And the event has a "state" breadcrumb named "Bugsnag loaded"
        And the event has a "manual" breadcrumb named "String breadcrumb clicked"
        And the event has a "navigation" breadcrumb named "Tuple breadcrumb clicked"

        # Context
        And the event "context" equals "My context"

        # MetaData
        And the event "metaData.device.osLanguage" is not null
        And the event "metaData.app.companyName" equals "bugsnag"
        And the event "metaData.app.name" equals "Mazerunner"
        And the event "metaData.User.test" equals "[REDACTED]"
        And the event "metaData.User.password" equals "[REDACTED]"
        
        # Runtime versions
        And the error payload field "events.0.device.runtimeVersions.osBuild" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null
    
    Scenario: Custom App Type
        When I run the "Custom App Type" mobile scenario
        Then I wait to receive an error
        And the event "app.type" equals "test"
