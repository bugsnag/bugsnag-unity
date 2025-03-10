Feature: csharp events on Android have a il2cpp addresses

  Background:
    Given I clear the Bugsnag cache

  @skip_unity_2020
  Scenario: Android Uncaught Exception has il2cpp addresses
    When I run the game in the "UncaughtExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "UncaughtExceptionSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event

    And the event "exceptions.0.stacktrace.0.loadAddress" equals "0x0"
    And the error payload field "events.0.exceptions.0.stacktrace.0.file" matches the regex ".*/libil2cpp.so$"
    And the error payload field "events.0.exceptions.0.stacktrace.0.codeIdentifier" matches the regex "[0-9a-fA-F]{40}"
    And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" matches the regex "0x[0-9a-fA-F]{1,16}"
    And the event "exceptions.0.stacktrace.0.isPC" is true
    And the event "exceptions.0.stacktrace.0.type" equals "c"

    And the event "exceptions.0.stacktrace.1.loadAddress" equals "0x0"
    And the error payload field "events.0.exceptions.0.stacktrace.1.file" matches the regex ".*/libil2cpp.so$"
    And the error payload field "events.0.exceptions.0.stacktrace.1.codeIdentifier" matches the regex "[0-9a-fA-F]{40}"
    And the error payload field "events.0.exceptions.0.stacktrace.1.frameAddress" matches the regex "0x[0-9a-fA-F]{1,16}"
    And the event "exceptions.0.stacktrace.1.isPC" is true
    And the event "exceptions.0.stacktrace.1.type" equals "c"

    And the event "exceptions.0.stacktrace.2.loadAddress" equals "0x0"
    And the error payload field "events.0.exceptions.0.stacktrace.1.file" matches the regex ".*/libil2cpp.so$"
    And the error payload field "events.0.exceptions.0.stacktrace.1.codeIdentifier" matches the regex "[0-9a-fA-F]{40}"
    And the error payload field "events.0.exceptions.0.stacktrace.1.frameAddress" matches the regex "0x[0-9a-fA-F]{1,16}"
    And the event "exceptions.0.stacktrace.2.isPC" is true
    And the event "exceptions.0.stacktrace.2.type" equals "c"

    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the error payload field "events.0.severityReason.unhandledOverridden" is false

  @skip_unity_2020
  Scenario: Android Notify Exception has il2cpp addresses
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "NotifySmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event

    And the event "exceptions.0.stacktrace.0.loadAddress" equals "0x0"
    And the error payload field "events.0.exceptions.0.stacktrace.0.file" matches the regex ".*/libil2cpp.so$"
    And the error payload field "events.0.exceptions.0.stacktrace.0.codeIdentifier" matches the regex "[0-9a-fA-F]{40}"
    And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" matches the regex "0x[0-9a-fA-F]{1,16}"
    And the event "exceptions.0.stacktrace.0.isPC" is true
    And the event "exceptions.0.stacktrace.0.type" equals "c"

    And the event "exceptions.0.stacktrace.1.loadAddress" equals "0x0"
    And the error payload field "events.0.exceptions.0.stacktrace.1.file" matches the regex ".*/libil2cpp.so$"
    And the error payload field "events.0.exceptions.0.stacktrace.1.codeIdentifier" matches the regex "[0-9a-fA-F]{40}"
    And the error payload field "events.0.exceptions.0.stacktrace.1.frameAddress" matches the regex "0x[0-9a-fA-F]{1,16}"
    And the event "exceptions.0.stacktrace.1.isPC" is true
    And the event "exceptions.0.stacktrace.1.type" equals "c"

    And the event "exceptions.0.stacktrace.2.loadAddress" equals "0x0"
    And the error payload field "events.0.exceptions.0.stacktrace.1.file" matches the regex ".*/libil2cpp.so$"
    And the error payload field "events.0.exceptions.0.stacktrace.1.codeIdentifier" matches the regex "[0-9a-fA-F]{40}"
    And the error payload field "events.0.exceptions.0.stacktrace.1.frameAddress" matches the regex "0x[0-9a-fA-F]{1,16}"
    And the event "exceptions.0.stacktrace.2.isPC" is true
    And the event "exceptions.0.stacktrace.2.type" equals "c"

    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the error payload field "events.0.severityReason.unhandledOverridden" is false
