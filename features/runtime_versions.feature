Feature: Runtime versions are included in reports

    Scenario: Handled error includes runtime versions
        When I run the game in the "Notify" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the payload field "events" is an array with 1 element
        And the payload field "events.0.device.runtimeVersions.unity" is not null
        And the payload field "events.0.device.runtimeVersions.unityScriptingBackend" is not null
        And the payload field "events.0.device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the payload field "events.0.device.runtimeVersions.dotnetApiCompatibility" is not null

    Scenario: Unhandled error includes runtime versions
        When I run the game in the "UncaughtException" state
        Then I should receive a request
        And the request is a valid for the error reporting API
        And the payload field "events.0.device.runtimeVersions.unity" is not null
        And the payload field "events.0.device.runtimeVersions.unityScriptingBackend" is not null
        And the payload field "events.0.device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the payload field "events.0.device.runtimeVersions.dotnetApiCompatibility" is not null

    Scenario: Session includes runtime versions
        When I run the game in the "ManualSession" state
        Then I should receive a request
        And the request is a valid for the session tracking API
        And the payload field "device.runtimeVersions.unity" is not null
        And the payload field "device.runtimeVersions.unityScriptingBackend" is not null
        And the payload field "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the payload field "device.runtimeVersions.dotnetApiCompatibility" is not null
