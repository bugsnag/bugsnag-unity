Feature: Android manual smoke tests

    Background:
        Given I wait for the mobile game to start

    Scenario: NDK Signal raised
        When I run the "NDK signal" mobile scenario
        And I wait for 8 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        # Intentionally adding long wait times here - a core component of this
        # feature is ensuring that only a SINGLE event is sent. Unity includes
        # a handler for NDK events which are delivered immediately and should
        # be intentionally ignored and the event discarded, since the same
        # event has already been captured and will be sent next launch
        And I wait for 5 seconds
        Then I wait to receive 1 error

        # Exception details
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "SIGSEGV"
        And the exception "message" equals "Segmentation violation (invalid memory reference)"
        And the exception "type" equals "c"
        And the event "unhandled" is true
        And the event "severity" equals "error"
        And the event "severityReason.type" equals "signal"
        And the event "severityReason.attributes.signalType" equals "SIGSEGV"
        And the event "severityReason.unhandledOverridden" is false

        # Stacktrace validation
        And the error payload field "events.0.exceptions.0.stacktrace" is a non-empty array
        And the event "exceptions.0.stacktrace.0.method" is not null
        And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" is greater than 0

        # App data
        And the event "app.id" equals "com.bugsnag.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "android"
        And the event "app.version" equals "1.2.3"
        And the event "app.versionCode" equals 1
        And the error payload field "events.0.app.duration" is not null
        And the error payload field "events.0.app.durationInForeground" is not null
        And the event "app.inForeground" is true
        And the event "app.isLaunching" is not null
        And the event "app.binaryArch" is not null

        # Device data
        And the error payload field "events.0.device.cpuAbi" is a non-empty array
        And the event "device.jailbroken" is false
        And the event "device.id" is not null
        And the event "device.locale" is not null
        And the event "device.manufacturer" is not null
        And the event "device.model" is not null
        And the event "device.osName" equals "android"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the error payload field "events.0.device.totalMemory" is not null
        And the event "device.orientation" equals "portrait"
        And the event "device.time" is not null

        # User
        And the event "user.id" is not null

        # Native context override
        And the event "context" equals "My context"

        # Metadata
        And the event "metaData.app.activeScreen" equals "UnityPlayerActivity"
        And the event "metaData.app.name" equals "Mazerunner"
        And the event "metaData.device.brand" equals "google"
        And the event "metaData.device.dpi" equals 440
        And the event "metaData.device.emulator" is false
        And the event "metaData.device.locationStatus" is not null
        And the event "metaData.device.networkAccess" is not null
        And the event "metaData.device.screenDensity" is not null
        And the event "metaData.device.screenResolution" is not null
        And the event "metaData.device.osLanguage" is not null
        And the event "app.type" equals "android"
        And the event "metaData.app.companyName" equals "bugsnag"
        And the event "metaData.app.name" equals "Mazerunner"
