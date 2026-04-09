Feature: Android Config

  Background:
    Given I clear the Bugsnag cache

  Scenario: Android Persistence Directory
    When I run the game in the "AndroidPersistenceDirectory" state
    And I wait to receive an error
    And the exception "message" equals "Directory Found"

  Scenario: Version Code From Player Settings
    When I run the game in the "AndroidVersionCodeInPlayerSettings" state
    And I wait to receive an error
    And the exception "message" equals "AndroidVersionCodeInPlayerSettings"
    And the event "app.versionCode" equals 444

  Scenario: Version code From Config
    When I run the game in the "AndroidVersionCodeInConfig" state
    And I wait to receive an error
    And the exception "message" equals "AndroidVersionCodeInConfig"
    And the event "app.versionCode" equals 555

  Scenario: Disable Automatic Context Capture
    When I run the game in the "AndroidDisableAutomaticContext" state
    And I wait to receive an error
    And the exception "message" equals "AndroidDisableAutomaticContext"
    And the event "context" is null

  Scenario: Enable Automatic Context Capture
    When I run the game in the "AndroidEnableAutomaticContext" state
    And I wait to receive an error
    And the exception "message" equals "AndroidEnableAutomaticContext"
    And the event "context" equals "UnityPipelineContext"
