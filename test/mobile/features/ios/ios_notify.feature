Feature: iOS smoke tests for C# errors

    # TODO: Failing steps commented out pending full implementation of PLAT-6372
    Background:
        Given I wait for the game to start

    Scenario: Calling Bugsnag.Notify() with caught C# exception

        When I tap the "Notify caught exception" button
        Then I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "IndexOutOfRangeException"
        And the exception "message" equals "Index was outside the bounds of the array."
        And the event "unhandled" is false
        And the event "severity" equals "warning"
        And the event "severityReason.type" equals "handledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" ends with "ReporterBehavior.NotifyCaughtException()"
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 0
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.unity.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "iOS"
        And the event "app.version" equals "1.2.3"
#        And the event "app.versionCode" equals "1"
#        And the error payload field "events.0.app.duration" is not null
        And the error payload field "events.0.app.durationInForeground" is not null
#        And the event "app.inForeground" equals "true"
#        And the event "app.isLaunching" equals "false"
#        And the event "app.memoryUsage" is not null
#        And the event "app.name" equals "maze_runner"
#        And the event "app.lowMemory" equals "false"

        # Device data
        And the event "device.jailbroken" is false
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" equals "Apple"
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
        And the event "metaData.app.companyName" equals "DefaultCompany"
        And the event "metaData.app.name" equals "maze_runner"

        # Runtime versions
        And the error payload field "events.0.device.runtimeVersions.osBuild" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null

    Scenario: Calling Bugsnag.Notify() with caught C# exception with callback

        When I tap the "Notify with callback" button
        Then I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "This one has a callback"
        And the event "unhandled" is false
        And the event "severity" equals "warning"
        And the event "severityReason.type" equals "handledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" ends with "ReporterBehavior.NotifyWithCallback()"
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 0
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.unity.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "iOS"
        And the event "app.version" equals "1.2.3"
#        And the event "app.versionCode" equals "1"
#        And the error payload field "events.0.app.duration" is not null
        And the error payload field "events.0.app.durationInForeground" is not null
#        And the event "app.inForeground" equals "true"
#        And the event "app.isLaunching" equals "false"
#        And the event "app.memoryUsage" is not null
#        And the event "app.name" equals "maze_runner"
#        And the event "app.lowMemory" equals "false"

        # Device data
        And the event "device.jailbroken" is false
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" equals "Apple"
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
        And the event "context" equals "Callback Context"

        # MetaData
        And the event "metaData.device.osLanguage" is not null
        And the event "metaData.app.companyName" equals "DefaultCompany"
        And the event "metaData.app.name" equals "maze_runner"
        And the event "metaData.Callback.region" equals "US"

        # Runtime versions
        And the error payload field "events.0.device.runtimeVersions.osBuild" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null
