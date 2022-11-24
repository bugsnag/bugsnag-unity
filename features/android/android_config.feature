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
    And the event "app.versionCode" equals 111

  Scenario: Version code From Config
    When I run the game in the "AndroidVersionCodeInConfig" state
    And I wait to receive an error
    And the exception "message" equals "AndroidVersionCodeInConfig"
    And the event "app.versionCode" equals 222

