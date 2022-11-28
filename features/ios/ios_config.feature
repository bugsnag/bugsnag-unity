Feature: Config

  Background:
    Given I clear the Bugsnag cache

  Scenario: Bundle Version From Player Settings

    When I run the game in the "IosBundleVersionFromPlayerSettings" state
    And I wait to receive an error
    And the exception "message" equals "IosBundleVersionFromPlayerSettings"
    And the event "app.bundleVersion" equals "333"

  Scenario: Bundle Version From Config

    When I run the game in the "IosBundleVersionInConfig" state
    And I wait to receive an error
    And the exception "message" equals "IosBundleVersionInConfig"
    And the event "app.bundleVersion" equals "222"

