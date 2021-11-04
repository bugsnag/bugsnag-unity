Feature: Callbacks

    Background:
        Given I wait for the mobile game to start
        And I clear all persistent data

    Scenario: Event Callback
        When I run the "Ios Signal" mobile scenario
        And I wait for 2 seconds
        And I relaunch the Unity mobile app
        When I run the "Native Event Callback" mobile scenario
        Then I wait to receive an error
        
        And the exception "errorClass" equals "test"

        #device metadata
        And the event "device.locale" equals "test"
        And the event "device.osName" equals "test"
        And the event "device.osVersion" equals "test"
        And the event "device.id" equals "test"
        And the event "device.model" equals "test"
        And the event "device.orientation" equals "test"
        And the event "device.manufacturer" equals "test"
        And the event "device.freeDisk" equals 123
        And the event "device.freeMemory" equals 123
        And the event "device.jailbroken" is true
        And the event "device.locale" equals "test"

        #app metadata
        And the event "app.id" equals "test"
        And the event "app.releaseStage" equals "test"
        And the event "app.type" equals "test"
        And the event "app.version" equals "test"
        And the event "app.bundleVersion" equals "test"
        And the event "app.binaryArch" equals "test"
        And the event "app.codeBundleId" equals "test"
        And the event "app.dsymUUIDs.0" equals "test"
        And the event "app.inForeground" is false
        And the event "app.isLaunching" is false

        And the event "exceptions.0.stacktrace.0.method" equals "test"