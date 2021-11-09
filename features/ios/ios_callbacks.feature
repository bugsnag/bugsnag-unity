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
        
        And the exception "errorClass" equals "ErrorClass"

        #device metadata
        And the event "device.osName" equals "OsName"
        And the event "device.osVersion" equals "OsVersion"
        And the event "device.id" equals "Id"
        And the event "device.model" equals "Model"
        And the event "device.orientation" equals "Orientation"
        And the event "device.manufacturer" equals "Manufacturer"
        And the event "device.freeDisk" equals 123
        And the event "device.freeMemory" equals 123
        And the event "device.jailbroken" is true
        And the event "device.locale" equals "Locale"

        #app metadata
        And the event "app.id" equals "Id"
        And the event "app.releaseStage" equals "ReleaseStage"
        And the event "app.type" equals "Type"
        And the event "app.version" equals "Version"
        And the event "app.bundleVersion" equals "BundleVersion"
        And the event "app.binaryArch" equals "BinaryArch"
        And the event "app.codeBundleId" equals "CodeBundleId"
        And the event "app.dsymUuid" equals "DsymUuid"
        And the event "app.inForeground" is false
        And the event "app.isLaunching" is false

        And the event "exceptions.0.stacktrace.0.method" equals "Method"