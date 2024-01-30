Feature: Native Errors

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Discard Native Errors
        When I run the "Discard Error Class" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK no errors" mobile scenario
        Then I should receive no errors

    Scenario: Native Error With Threads
        When I run the "iOS Native Error" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.threads" is a non-empty array

    Scenario: Native Error Without Threads
        When I run the "iOS Native Error No Threads" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Start SDK" mobile scenario
        Then I wait to receive an error
        And the error payload field "events.0.threads.0" is null
        And the error payload field "notifier.dependencies.0.name" equals "iOS Bugsnag Notifier"

        # App data
        And the event "app.id" equals "com.bugsnag.unity.mazerunner"
        And the event "app.releaseStage" equals "production"
        And the event "app.type" equals "iOS"
        And the event "app.version" equals "1.2.3"
        And the event "app.bundleVersion" equals "1.2.3"
        And the event "app.isLaunching" is true
        And the event "app.inForeground" is true
        And the event "app.durationInForeground" is greater than 0
        And the event "app.duration" is greater than 0
        And the event "app.binaryArch" is not null

        # Device data
        And the event "device.jailbroken" is false
        And the event "device.id" is not null
        And the event "device.locale" is not null
        And the event "device.manufacturer" is not null
        And the event "device.model" is not null
        And the event "device.osName" equals "iOS"
        And the event "device.osVersion" is not null
        And the event "device.runtimeVersions" is not null
        And the event "device.totalMemory" is not null
        And the event "device.freeMemory" is not null
        And the event "device.time" is not null


    Scenario: Last Run Info
        When I run the "iOS Native Error" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Check Last Run Info" mobile scenario
        Then I wait to receive 2 errors
        And I discard the oldest error
        And the exception "message" equals "Last Run Info Correct"