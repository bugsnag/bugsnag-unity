Feature: csharp events

  Scenario: Notify smoke test
    When I run the game in the "NEWNotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "NotifySmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | NotifySmokeTest.Run() | 
      | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | 

    # Device metadata
    And the event "device.freeDisk" is not null
    And the event "device.freeMemory" is not null
    And the event "device.id" is not null
    And the event "device.locale" is not null
    And the event "device.manufacturer" is not null
    And the event "device.model" is not null
    And the event "device.osName" is not null
    And the event "device.osVersion" is not null
    And the event "device.runtimeVersions" is not null
    And the event "device.time" is a timestamp
    And the event "device.totalMemory" is not null

    # Auto metadata
    And the event "metaData.device.screenDensity" is not null
    And the event "metaData.device.screenResolution" is not null
    And the event "metaData.device.osLanguage" equals "English"
    And the event "metaData.device.graphicsDeviceVersion" is not null
    And the event "metaData.device.graphicsMemorySize" is not null
    And the event "metaData.device.processorType" is not null

    # App metadata
    And the event "app.duration" is greater than 0
    And the event "app.durationInForeground" is not null
    And the event "app.inForeground" is not null
    And the event "app.isLaunching" is not null
    And the event "app.releaseStage" is not null
    And the event "app.type" is not null
    And the event "app.version" is not null

    # Auto app data
    And the event "metaData.app.companyName" equals "bugsnag"
    And the event "metaData.app.name" equals "Mazerunner"
    And the event "metaData.app.buildno" is not null


