Feature: csharp events

  Background:
    Given I clear the Bugsnag cache

  Scenario: Notify smoke test
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "NotifySmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | NotifySmokeTest.Run()                                                       | Main+<RunNextMazeCommand>d__8.MoveNext() |
    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the error payload field "events.0.severityReason.unhandledOverridden" is false


  Scenario: Uncaught Exception smoke test
    When I run the game in the "UncaughtExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "UncaughtExceptionSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | UncaughtExceptionSmokeTest.Run()                                                                 |                                          |
      | ScenarioRunner.RunScenario(System.String scenarioName, System.String apiKey, System.String host) | Main+<RunNextMazeCommand>d__8.MoveNext() |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

  @ios_only
  @skip_before_unity_2021
  Scenario: Uncaught Exception ios smoke test
    When I run the game in the "UncaughtExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "UncaughtExceptionSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And expected device metadata is included in the event
    And expected app metadata is included in the event
    And the error payload field "events.0.exceptions.0.stacktrace.0.frameAddress" matches the regex "\d+"
    And the error payload field "events.0.exceptions.0.stacktrace.0.method" equals "UncaughtExceptionSmokeTest.Run()"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoFile" matches the regex ".*/UnityFramework.framework/UnityFramework"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoLoadAddress" matches the regex "\d+"
    And the error payload field "events.0.exceptions.0.stacktrace.0.machoUUID" matches the regex "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"
    And the error payload field "events.0.exceptions.0.stacktrace.0.inProject" is true

  Scenario: Debug Log Exception smoke test
    When I run the game in the "DebugLogExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "DebugLogExceptionSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | DebugLogExceptionSmokeTest:Run()                   | DebugLogExceptionSmokeTest.Run()                                            | <RunNextMazeCommand>d__8:MoveNext()                            | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) | UnityEngine.SetupCoroutine.InvokeMoveNext(IEnumerator enumerator, IntPtr returnValueAddress) |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

  Scenario: Debug Log Error smoke test
    When I run the game in the "DebugLogErrorSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogError"
    And the exception "message" equals "DebugLogErrorSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | DebugLogErrorSmokeTest:Run()                       | DebugLogErrorSmokeTest.Run()                                                | <RunNextMazeCommand>d__8:MoveNext()              |                                                                |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

  Scenario: Debug Log Warning smoke test
    When I run the game in the "DebugLogWarningSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogWarning"
    And the exception "message" equals "DebugLogWarningSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | DebugLogWarningSmokeTest:Run()                     | DebugLogWarningSmokeTest.Run()                                              | <RunNextMazeCommand>d__8:MoveNext()              |                                                                |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

  Scenario: Debug Log smoke test
    When I run the game in the "DebugLogSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogLog"
    And the exception "message" equals "DebugLogSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | DebugLogSmokeTest:Run()                            | DebugLogSmokeTest.Run()                                                     | <RunNextMazeCommand>d__8:MoveNext()              |                                                                |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

  Scenario: Debug Log Assert smoke test
    When I run the game in the "DebugLogAssertSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "UnityLogAssert"
    And the exception "message" equals "DebugLogAssertSmokeTest"
    And the event "unhandled" is false
    And custom metadata is included in the event
    And the stack frame methods should match:
      | DebugLogAssertSmokeTest:Run()                      | DebugLogAssertSmokeTest.Run()                                               | <RunNextMazeCommand>d__8:MoveNext()              |                                                                |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) | UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr) |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

  @skip_unity_2018
  Scenario: Reporting an inner exception
    When I run the game in the "InnerException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the event "exceptions.0.message" equals "Outer"
    And the event "exceptions.1.message" equals "Inner"
    And the event "exceptions.2" is null

  Scenario: Reporting an uncaught exception in an async method
    When I run the game in the "AsyncException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "AsyncException"

  @skip_webgl
  Scenario: Report exception from background thread
    When I run the game in the "BackgroundThreadException" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "BackgroundThreadException"

  @skip_webgl
  Scenario: Notify from background thread
    When I run the game in the "NotifyFromBackgroundThread" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "NotifyFromBackgroundThread"

  @skip_macos #impossible to reliably start the fixture in the foreground
  Scenario: Session present after start
    When I run the game in the "SessionAfterStart" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "SessionAfterStart"
    And the event "session" is not null

  Scenario: Notify with custom stacktrace
    When I run the game in the "NotifyWithCustomStacktrace" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "NotifyWithCustomStacktrace"
    And the stack frame methods should match:
      | Main.CUSTOM1() |
      | Main.CUSTOM2() |

  Scenario: Notify with strings
    When I run the game in the "NotifyWithStrings" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "name"
    And the exception "message" equals "NotifyWithStrings"
    And the stack frame methods should match:
      | Main.CUSTOM1() |
      | Main.CUSTOM2() |

  Scenario: Reporting a handled exception with a custom severity
    When I run the game in the "CustomSeverity" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "CustomSeverity"
    And the event "severity" equals "info"
    And the event "severityReason.type" equals "userSpecifiedSeverity"
    And the event "unhandled" is false

  Scenario: Discard event after serialisation error
    When I run the game in the "SerialisationError" state
    And I wait to receive 1 error
    And the exception "message" equals "SerialisationError"



