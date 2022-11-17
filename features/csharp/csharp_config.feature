Feature: csharp events

  Background:
    Given I clear the Bugsnag cache

#IS LAUNCHING SECTION --------------------------------------------------------------------
# These duplicate isLaunching tests will be merged after the bug PLAT-9060 is fixed
  #macos tests to accommodate bug
  @cocoa_only
  Scenario: Launch duration set to 0
    When I run the game in the "InfiniteLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfiniteLaunchDuration"
    And the event "app.isLaunching" equals "true"

  @cocoa_only
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

  @cocoa_only
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

  @cocoa_only
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
  @skip_cocoa
  Scenario: Launch duration set to 0
    When I run the game in the "InfiniteLaunchDuration" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "InfiniteLaunchDuration"
    And the event "app.isLaunching" is true

  @skip_cocoa
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

  @skip_cocoa @skip_windows @skip_webgl # PLAT-9061
  Scenario: Set long launch time
    When I run the game in the "LongLaunchTime" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the event "app.isLaunching" is true
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "app.isLaunching" is false

  @skip_cocoa @skip_windows @skip_webgl # PLAT-9061
  Scenario: Set short launch time
    When I run the game in the "ShortLaunchTime" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the event "app.isLaunching" is true
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "app.isLaunching" is false


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

  @skip_android #Pending PLAT-9210
  Scenario: Max String Value Length
    When I run the game in the "MaxStringValueLength" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "MaxStringValueLength"
    And the event "metaData.test.key-9483" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "metaData.test.stringArray.0" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "metaData.test.stringList.0" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "metaData.test.dictionary.stringArray.0" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "metaData.test.stringDictionary.testKey" equals "12345678901234567890***80 CHARS TRUNCATED***"

    And the event "breadcrumbs.1.metaData.testKey" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "breadcrumbs.1.metaData.stringArray.0" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "breadcrumbs.1.metaData.stringList.0" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "breadcrumbs.1.metaData.dictionary.stringArray.0" equals "12345678901234567890***80 CHARS TRUNCATED***"
    And the event "breadcrumbs.1.metaData.stringDictionary.testKey" equals "12345678901234567890***80 CHARS TRUNCATED***"





