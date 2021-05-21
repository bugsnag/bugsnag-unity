Feature: Runtime versions are included in reports

    Scenario: Handled error includes runtime versions
        When I run the game in the "Notify" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the event "device.runtimeVersions.unity" is not null
        And the event "device.runtimeVersions.unityScriptingBackend" is not null
        And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the event "device.runtimeVersions.dotnetApiCompatibility" is not null

    Scenario: Unhandled error includes runtime versions
        When I run the game in the "UncaughtException" state
        And I wait to receive an error
        Then the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the event "device.runtimeVersions.unity" is not null
        And the event "device.runtimeVersions.unityScriptingBackend" is not null
        And the event "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the event "device.runtimeVersions.dotnetApiCompatibility" is not null

    Scenario: Session includes runtime versions
        When I run the game in the "ManualSession" state
        And I wait to receive a session
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the session payload field "device.runtimeVersions.unity" is not null
        And the session payload field "device.runtimeVersions.unityScriptingBackend" is not null
        And the session payload field "device.runtimeVersions.dotnetScriptingRuntime" is not null
        And the session payload field "device.runtimeVersions.dotnetApiCompatibility" is not null
