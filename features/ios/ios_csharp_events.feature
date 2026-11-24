Feature: csharp events on iOS have a il2cpp addresses

  Background:
    Given I clear the Bugsnag cache

  @skip_unity_2020
  Scenario: iOS Uncaught Exception has il2cpp addresses
    When I run the game in the "UncaughtExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "type" equals "cocoa"
    And the exception "message" equals "UncaughtExceptionSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event

    And the error payload field "events.0.exceptions.0.stacktrace.0.machoFile" matches the regex ".*UnityFramework.*"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoUUID" matches the regex "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" matches the regex "[0-9a-fA-F]{1,32}"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoLoadAddress" matches the regex "[0-9a-fA-F]{1,32}"

    And the error payload field "events.0.exceptions.0.stacktrace.1.machoFile" matches the regex ".*UnityFramework.*"
    And the error payload field "events.0.exceptions.0.stacktrace.1.machoUUID" matches the regex "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    And the error payload field "events.0.exceptions.0.stacktrace.1.frameAddress" matches the regex "[0-9a-fA-F]{1,32}"
    And the error payload field "events.0.exceptions.0.stacktrace.1.machoLoadAddress" matches the regex "[0-9a-fA-F]{1,32}"

    And the error payload field "events.0.exceptions.0.stacktrace.2.machoFile" matches the regex ".*UnityFramework.*"
    And the error payload field "events.0.exceptions.0.stacktrace.2.machoUUID" matches the regex "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    And the error payload field "events.0.exceptions.0.stacktrace.2.frameAddress" matches the regex "[0-9a-fA-F]{1,32}"
    And the error payload field "events.0.exceptions.0.stacktrace.2.machoLoadAddress" matches the regex "[0-9a-fA-F]{1,32}"

    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the error payload field "events.0.severityReason.unhandledOverridden" is false

  @skip_unity_2020
  Scenario: ios Notify Exception has il2cpp addresses
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "type" equals "cocoa"
    And the exception "message" equals "NotifySmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event

    And the error payload field "events.0.exceptions.0.stacktrace.0.machoFile" matches the regex ".*UnityFramework.*"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoUUID" matches the regex "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" matches the regex "[0-9a-fA-F]{1,32}"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoLoadAddress" matches the regex "[0-9a-fA-F]{1,32}"

    And the error payload field "events.0.exceptions.0.stacktrace.1.machoFile" matches the regex ".*UnityFramework.*"
    And the error payload field "events.0.exceptions.0.stacktrace.1.machoUUID" matches the regex "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    And the error payload field "events.0.exceptions.0.stacktrace.1.frameAddress" matches the regex "[0-9a-fA-F]{1,32}"
    And the error payload field "events.0.exceptions.0.stacktrace.1.machoLoadAddress" matches the regex "[0-9a-fA-F]{1,32}"

    And the error payload field "events.0.exceptions.0.stacktrace.2.machoFile" matches the regex ".*UnityFramework.*"
    And the error payload field "events.0.exceptions.0.stacktrace.2.machoUUID" matches the regex "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    And the error payload field "events.0.exceptions.0.stacktrace.2.frameAddress" matches the regex "[0-9a-fA-F]{1,32}"
    And the error payload field "events.0.exceptions.0.stacktrace.2.machoLoadAddress" matches the regex "[0-9a-fA-F]{1,32}"

    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the error payload field "events.0.severityReason.unhandledOverridden" is false

  Scenario: iOS managed frames have inProject set from the default namespaces
    When I run the game in the "InProjectDefaults" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InProjectDefaults"
    And every stack frame has a boolean inProject
    And the stack frames with methods matching "^InProjectDefaults" have inProject true
    And the stack frames with methods matching "^System\.Linq" have inProject false

  Scenario: iOS managed frames have inProject set from configured namespaces
    When I run the game in the "InProjectCustomConfig" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InProjectCustomConfig"
    And every stack frame has a boolean inProject
    And the stack frames with methods matching "^InProjectCustomConfig" have inProject false
    And the stack frames with methods matching "^System\.Linq" have inProject true
