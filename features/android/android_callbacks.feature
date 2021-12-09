Feature: Callbacks

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Session Callback
        When I run the "Session Callback" mobile scenario
        And I wait to receive a session

        # Session data
        And the session payload field "sessions.0.id" equals "Custom Id"
        And the session payload field "sessions.0.startedAt" equals "1985-08-21T01:01:01.000Z"
        And the session payload field "sessions.0.user.id" equals "1"
        And the session payload field "sessions.0.user.email" equals "2"
        And the session payload field "sessions.0.user.name" equals "3"


        # App Data
        And the session payload field "app.binaryArch" equals "Custom BinaryArch"
        And the session payload field "app.buildUUID" equals "Custom BuildUuid"
        And the session payload field "app.codeBundleId" equals "Custom CodeBundleId" 
        And the session payload field "app.id" equals "Custom Id"
        And the session payload field "app.releaseStage" equals "Custom ReleaseStage"
        And the session payload field "app.type" equals "Custom Type"
        And the session payload field "app.version" equals "Custom Version"
        And the session payload field "app.versionCode" equals 999


        # Device data
        And the session payload field "device.cpuAbi" is a non-empty array
        And the session payload field "device.jailbroken" is true
        And the session payload field "device.id" equals "Custom Device Id"
        And the session payload field "device.locale" equals "Custom Locale"
        And the session payload field "device.manufacturer" equals "Custom Manufacturer"
        And the session payload field "device.model" equals "Custom Model"
        And the session payload field "device.osName" equals "Custom OsName"
        And the session payload field "device.osVersion" equals "Custom OsVersion"
        And the session payload field "device.runtimeVersions" is not null
        And the session payload field "device.totalMemory" equals 999

     Scenario: On Send Native Callback
        When I run the "Java Background Crash" mobile scenario
        And I wait for 4 seconds
        And I relaunch the Unity mobile app
        When I run the "On Send Native Callback" mobile scenario
        Then I wait to receive an error

        And the error payload field "apiKey" equals "Custom ApiKey"

        # misc data
        And the event "context" equals "Custom Context"
        And the event "severity" equals "info"
        And the event "unhandled" is false

        # metadata
        And the event "metaData.test.scoop" equals "dewoop"
        And the event "metaData.test2" is null

        # user
        And the event "user.id" equals "1"
        And the event "user.email" equals "2"
        And the event "user.name" equals "3"

        # App Data
        And the event "app.binaryArch" equals "Custom BinaryArch"
        And the event "app.buildUUID" equals "Custom BuildUuid"
        And the event "app.codeBundleId" equals "Custom CodeBundleId" 
        And the event "app.id" equals "Custom Id"
        And the event "app.releaseStage" equals "Custom ReleaseStage"
        And the event "app.type" equals "Custom Type"
        And the event "app.version" equals "Custom Version"
        And the event "app.versionCode" equals 999
        And the event "app.duration" equals 1000
        And the event "app.durationInForeground" equals 1000
        And the event "app.inForeground" is false
        And the event "app.isLaunching" is false

        # Device data
        And the error payload field "events.0.device.cpuAbi" is an array with 2 elements
        And the event "device.jailbroken" is true
        And the event "device.id" equals "Custom Device Id"
        And the event "device.locale" equals "Custom Locale"
        And the event "device.manufacturer" equals "Custom Manufacturer"
        And the event "device.model" equals "Custom Model"
        And the event "device.osName" equals "Custom OsName"
        And the event "device.osVersion" equals "Custom OsVersion"
        And the event "device.totalMemory" equals 999
        And the event "device.runtimeVersions.scoop" equals "dewoop"
        And the event "device.time" equals "1985-08-21T01:01:01.000Z"
        And the event "device.orientation" equals "Custom Orientation"

        # Breadcrumbs
        And the event "breadcrumbs.0.name" equals "Custom Message"
        And the event "breadcrumbs.0.type" equals "user"
        And the event "breadcrumbs.0.metaData.Custom" equals "Metadata"

        # threads
        And the event "threads.0.name" equals "Custom Name"

        # errors
        And the event "exceptions.0.errorClass" equals "Custom ErrorClass"
        And the event "exceptions.0.message" equals "Custom ErrorMessage"

        # stacktrace
        And the event "exceptions.0.stacktrace.0.method" equals "Custom Method"
        And the event "exceptions.0.stacktrace.0.file" equals "Custom File"
        And the event "exceptions.0.stacktrace.0.lineNumber" equals 123123
        And the event "exceptions.0.stacktrace.0.inProject" is false








