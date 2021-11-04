Feature: Callbacks

    Scenario: CSharp event callback
        When I run the game in the "EventCallbacks" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the exception "errorClass" equals "test"
        And the exception "message" equals "blorb"

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
        And the event "app.dsymUuid" equals "test"
        And the event "app.inForeground" is false
        And the event "app.isLaunching" is false


        
        
