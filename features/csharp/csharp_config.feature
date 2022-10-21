Feature: csharp events

#IS LAUNCHING SECTION --------------------------------------------------------------------
# These duplicate isLaunching tests will be merged after the bug PLAT-9060 is fixed
 
  #macos tests to accommodate bug
  @macos_only
  Scenario: Launch duration set to 0
    When I run the game in the "InfiniteLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfiniteLaunchDuration"
    And the event "app.isLaunching" equals "true"
  
  @macos_only
  Scenario: Call mark launch complete
    When I run the game in the "MarkLaunchComplete" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the event "app.isLaunching" equals "true"
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "app.isLaunching" equals "false"
  
  @macos_only
  Scenario: Set long launch time
    When I run the game in the "LongLaunchTime" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the event "app.isLaunching" equals "true"
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "app.isLaunching" equals "false"
  
  @macos_only
  Scenario: Set short launch time
    When I run the game in the "ShortLaunchTime" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the event "app.isLaunching" equals "true"
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "app.isLaunching" equals "false"


#Correct tests to be enabled when PLAT-9061 is fixed
#skip android pending PLAT-9087

  @skip_macos 
  Scenario: Launch duration set to 0
    When I run the game in the "InfiniteLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfiniteLaunchDuration"
    And the event "app.isLaunching" is true
  
  @skip_macos 
  Scenario: Call mark launch complete
    When I run the game in the "MarkLaunchComplete" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the event "app.isLaunching" is true
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "app.isLaunching" is false
  
  # Scenario: Set long launch time
  #   When I run the game in the "LongLaunchTime" state
  #   And I wait to receive 2 errors
  #   And I sort the errors by the payload field "events.0.exceptions.0.message"
  #   Then the error is valid for the error reporting API sent by the Unity notifier
  #   And the exception "message" equals "Error 1"
  #   And the event "app.isLaunching" is true
  #   And I discard the oldest error
  #   And the exception "message" equals "Error 2"
  #   And the event "app.isLaunching" is false
  
  # Scenario: Set short launch time
  #   When I run the game in the "ShortLaunchTime" state
  #   And I wait to receive 2 errors
  #   And I sort the errors by the payload field "events.0.exceptions.0.message"
  #   Then the error is valid for the error reporting API sent by the Unity notifier
  #   And the exception "message" equals "Error 1"
  #   And the event "app.isLaunching" is true
  #   And I discard the oldest error
  #   And the exception "message" equals "Error 2"
  #   And the event "app.isLaunching" is false


#END SECTION -----------------------------------------------------------------------------

  Scenario: Auto notify false
    When I run the game in the "AutoNotifyFalse" state
    And I should receive no errors

  Scenario: Release Stage
    When I run the game in the "ReleaseStage" state
    And I should receive no errors

  Scenario: Disable UnityLog Error type
    When I run the game in the "DisableUnityLogError" state
    And I should receive no errors

  Scenario: Set Uncaught As Unhandled
    When I run the game in the "SetUncaughtAsUnhandled" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "SetUncaughtAsUnhandled"
    And the event "unhandled" is true

  Scenario: Discard Error Class
    When I run the game in the "DiscardErrorClass" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 2"

  Scenario: App Type
    When I run the game in the "AppType" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "AppType"
    And the event "app.type" equals "custom"

  Scenario: Redacted Keys
    When I run the game in the "RedactedKeys" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "RedactedKeys"
    And the event "metaData.testSection.testKey" equals "[REDACTED]"

  Scenario: Context
    When I run the game in the "Context" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Context"
    And the event "context" equals "test"


    



