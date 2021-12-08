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


        # App Data
        And the error payload field "app.binaryArch" equals "Custom BinaryArch"
        And the error payload field "app.buildUUID" equals "Custom BuildUuid"
        And the error payload field "app.codeBundleId" equals "Custom CodeBundleId" 
        And the error payload field "app.id" equals "Custom Id"
        And the error payload field "app.releaseStage" equals "Custom ReleaseStage"
        And the error payload field "app.type" equals "Custom Type"
        And the error payload field "app.version" equals "Custom Version"
        And the error payload field "app.versionCode" equals 999


        # Device data
        And the error payload field "device.cpuAbi" is a non-empty array
        And the error payload field "device.jailbroken" is true
        And the error payload field "device.id" equals "Custom Device Id"
        And the error payload field "device.locale" equals "Custom Locale"
        And the error payload field "device.manufacturer" equals "Custom Manufacturer"
        And the error payload field "device.model" equals "Custom Model"
        And the error payload field "device.osName" equals "Custom OsName"
        And the error payload field "device.osVersion" equals "Custom OsVersion"
        And the error payload field "device.runtimeVersions" is not null
        And the error payload field "device.totalMemory" equals 999
