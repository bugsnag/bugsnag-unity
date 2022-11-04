Feature: Feature Flags

  Background:
    Given I clear the Bugsnag cache

  Scenario: Add Feature Flags in config
    When I run the game in the "FeatureFlagsInConfig" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "FeatureFlagsInConfig"
    And the event "featureFlags.0.featureFlag" equals "testName2"
    And the event "featureFlags.0.variant" equals "testVariant2"
    And the event "featureFlags.1" is null

  Scenario: Add Feature Flags in config then clear all
    When I run the game in the "FeatureFlagsConfigClearAll" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "FeatureFlagsConfigClearAll"
    And the event "featureFlags.0" is null

  Scenario: Add Feature Flags after init
    When I run the game in the "FeatureFlagsAfterInit" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "FeatureFlagsAfterInit"
    And the event "featureFlags.0.featureFlag" equals "testName2"
    And the event "featureFlags.0.variant" equals "testVariant2"
    And the event "featureFlags.1" is null

  Scenario: Add Feature Flags after init then clear all
    When I run the game in the "FeatureFlagsAfterInitClearAll" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "FeatureFlagsAfterInitClearAll"
    And the event "featureFlags.0" is null

  Scenario: Add Feature Flags in callback
    When I run the game in the "FeatureFlagsInCallback" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "FeatureFlagsInCallback"
    And the event "featureFlags.1.featureFlag" equals "testName3"
    And the event "featureFlags.1.variant" equals "testVariant3"
    And the event "featureFlags.0.featureFlag" equals "testName2"
    And the event "featureFlags.0.variant" equals "testVariant2"
    And the event "featureFlags.2" is null

  Scenario: Clear All Feature Flags in callback
    When I run the game in the "ClearFeatureFlagsInCallback" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "ClearFeatureFlagsInCallback"
    And the event "featureFlags.0" is null
