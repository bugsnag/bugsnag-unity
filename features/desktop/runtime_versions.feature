Feature: Runtime versions are included in reports

    Scenario: Handled error includes runtime versions
        When I run the game in the "Notify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the event "device.runtimeVersions.unity" is not null
        And the event "device.runtimeVersions.unityScriptingBackend" is not null
        And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the event "device.runtimeVersions.dotnetApiCompatibility" is not null

    Scenario: Unhandled error includes runtime versions
        When I run the game in the "UncaughtException" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the Unity notifier
        And the event "device.runtimeVersions.unity" is not null
        And the event "device.runtimeVersions.unityScriptingBackend" is not null
        And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the event "device.runtimeVersions.dotnetApiCompatibility" is not null

