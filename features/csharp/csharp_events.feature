Feature: csharp events

  Scenario: Notify smoke test
    When I run the game in the "NotifySmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "NotifySmokeTest"
    And the event "unhandled" is false
    
    And custom metadata is included in the event
    
    And the stack frame methods should match:
      | NotifySmokeTest.Run() | 
      | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | 

    And expected device metadata is included in the event
    And expected app metadata is included in the event

  Scenario: Uncaught Exception smoke test
    When I run the game in the "UncaughtExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "UncaughtExceptionSmokeTest"
    And the event "unhandled" is false
    
    And custom metadata is included in the event
    
    And the stack frame methods should match:
      | UncaughtExceptionSmokeTest.Run() | 
      | ScenarioRunner.RunScenario(System.String scenarioName, System.String apiKey, System.String host) | 

    And expected device metadata is included in the event
    And expected app metadata is included in the event

Scenario: Debug Log Exception smoke test
    When I run the game in the "DebugLogExceptionSmokeTest" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "errorClass" equals "Exception"
    And the exception "message" equals "DebugLogExceptionSmokeTest"
    And the event "unhandled" is false
    
    And custom metadata is included in the event
    
    And the stack frame methods should match:
      | DebugLogExceptionSmokeTest:Run()| DebugLogExceptionSmokeTest.Run() |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) |

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
      | DebugLogErrorSmokeTest:Run() | DebugLogErrorSmokeTest.Run() ||
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) |


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
      | DebugLogWarningSmokeTest:Run() | DebugLogWarningSmokeTest.Run() |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) |
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
      | DebugLogSmokeTest:Run() | DebugLogSmokeTest.Run() |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) |
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
      | DebugLogAssertSmokeTest:Run() | DebugLogAssertSmokeTest.Run() |
      | ScenarioRunner:RunScenario(String, String, String) | ScenarioRunner.RunScenario(string scenarioName, string apiKey, string host) | ScenarioRunner:RunScenario(string,string,string) |
    And expected device metadata is included in the event
    And expected app metadata is included in the event

# @skip_unity_2018
# Scenario: Reporting an inner exception
#     When I run the game in the "InnerException" state
#     And I wait to receive an error
#     Then the error is valid for the error reporting API sent by the Unity notifier
#     And the event "exceptions.0.message" equals "Outer"
#     And the event "exceptions.1.message" equals "Inner"
#     And the event "exceptions.2" is null