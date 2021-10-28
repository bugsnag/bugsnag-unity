Feature: Android manual smoke tests

    Background:
        Given I wait for the mobile game to start

 Scenario: Discard JVM Error Class
        When I run the "Discard Error Class" mobile scenario
        Then I should receive no errors

    Scenario: Uncaught JVM exception
        When I run the "Native exception" mobile scenario
        Then I wait to receive 1 error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "java.lang.ArrayIndexOutOfBoundsException"
        And the exception "message" equals "length=2; index=2"
        And the event "unhandled" is true
        And the event "severity" equals "error"
        And the error payload field "events.0.projectPackages" is a non-empty array
        And the event "severityReason.type" equals "unhandledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" equals "com.example.bugsnagcrashplugin.CrashHelper.triggerJvmException()"
        And the exception "stacktrace.0.file" equals "CrashHelper.java"
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 13
        And the error payload field "events.0.threads" is null

        # App data
        And the event "app.id" equals "com.bugsnag.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "android"
        And the event "app.version" equals "1.2.3"
        And the event "app.versionCode" equals "123"
        And the error payload field "events.0.app.duration" is not null
        And the error payload field "events.0.app.durationInForeground" is not null
        And the event "app.inForeground" equals "true"
        And the error payload field "events.0.app.memoryUsage" is not null
        And the event "app.name" equals "Mazerunner"
        And the event "app.lowMemory" equals "false"

        # Device data
        And the error payload field "events.0.device.cpuAbi" is a non-empty array
        And the event "device.jailbroken" equals "false"
        And the event "device.id" is not null
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the event "device.locale" is not null
        And the event "device.manufacturer" is not null
        And the event "device.model" is not null
        And the event "device.osName" equals "android"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the error payload field "events.0.device.totalMemory" is not null
        And the error payload field "events.0.device.freeDisk" is not null
        And the error payload field "events.0.device.freeMemory" is not null
        And the event "device.orientation" equals "portrait"
        And the event "device.time" is not null
        And the event "device.locationStatus" is not null
        And the event "device.emulator" equals "false"
        And the event "device.networkAccess" is not null
        And the event "device.charging" is not null
        And the event "device.screenDensity" is not null
        And the event "device.dpi" is not null
        And the event "device.screenResolution" is not null
        And the event "device.brand" is not null
        And the event "device.batteryLevel" is not null

        # User
        And the event "user.id" is not null

        # Breadcrumbs
        And the event has a "state" breadcrumb named "Bugsnag loaded"
        And the event has a "manual" breadcrumb named "String breadcrumb clicked"
        And the event has a "navigation" breadcrumb named "Tuple breadcrumb clicked"

        # Context
        And the event "context" equals "My context"

        # MetaData
        And the event "metaData.device.osLanguage" is present from Unity 2018
        And the event "app.type" equals "android"
        And the event "metaData.app.companyName" equals "bugsnag"
        And the event "metaData.app.name" equals "Mazerunner"


        # Runtime versions
        And the error payload field "events.0.device.runtimeVersions.androidApiLevel" is not null
        And the error payload field "events.0.device.runtimeVersions.osBuild" is not null
        And the error payload field "events.0.device.runtimeVersions.unity" is not null

    Scenario: Background JVM exception
        When I run the "Java Background Crash" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "java.lang.RuntimeException"
        And the exception "message" equals "Uncaught JVM exception from background thread"
        And the event "unhandled" is true
        And the event "severity" equals "error"
        And the error payload field "events.0.projectPackages" is a non-empty array
        And the event "severityReason.type" equals "unhandledException"

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the exception "stacktrace.0.file" equals "CrashHelper.java"
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 19
        And the error payload field "events.0.threads" is not null

        # App data
        And the event "app.id" equals "com.bugsnag.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "android"
        And the event "app.version" equals "1.2.3"
        And the event "app.versionCode" equals 123
        And the error payload field "events.0.app.duration" is not null
        And the error payload field "events.0.app.durationInForeground" is not null
        And the event "metaData.User.test" equals "[REDACTED]"
        And the event "metaData.User.password" equals "[REDACTED]"
        And the error payload field "events.0.threads" is a non-empty array

    Scenario: Background JVM exception no threads
        When I run the "Java Background Crash No Threads" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.threads.0" is null

    # Skip Unity 2017 as an extra SIGSEGV is sometimes sent
    @skip_unity_2017
    Scenario: Last Run Info
        When I run the "Java Background Crash" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Check Last Run Info" mobile scenario
        Then I wait to receive 2 errors
        And I discard the oldest error
        And the exception "message" equals "Last Run Info Correct"


