Feature: Fallback Breadcrumbs

  Scenario: Disabling Breadcrumbs
    When I run the game in the "DisableBreadcrumbs" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "DisableBreadcrumbs"
    And the event "breadcrumbs.0" is null

  Scenario: Enable Specific Breadcrumbs
    When I run the game in the "EnableBreadcrumbs" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    And I discard the oldest error

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error2"
    And the event "breadcrumbs.0.name" equals "Debug.Log"


  Scenario: Setting max breadcrumbs
    When I run the game in the "MaxBreadcrumbs" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "MaxBreadcrumbs"
    And the error payload field "events.0.breadcrumbs" is an array with 5 elements
    And the event "breadcrumbs.0.name" equals "Crumb 5"
    And the event "breadcrumbs.1.name" equals "Crumb 6"
    And the event "breadcrumbs.2.name" equals "Crumb 7"
    And the event "breadcrumbs.3.name" equals "Crumb 8"
    And the event "breadcrumbs.4.name" equals "Crumb 9"


  # skip android pending PLAT-9087
  @skip_android
  Scenario: Manual and Automatic Breadcrumbs
    When I run the game in the "ManualAndAutoBreadcrumbs" state
    And I wait to receive 3 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    And I discard the oldest error
    And I discard the oldest error

    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error3"

    And the event "breadcrumbs.0.name" equals "Bugsnag loaded"
    And the event "breadcrumbs.0.type" equals "state"


    And the event "breadcrumbs.1.name" equals "Manual"
    And the event "breadcrumbs.1.type" equals "manual"

    And the event "breadcrumbs.2.name" equals "Debug.Log"
    And the event "breadcrumbs.2.type" equals "log"

    And the event "breadcrumbs.3.name" equals "Debug.LogWarning"
    And the event "breadcrumbs.3.type" equals "log"

    And the event "breadcrumbs.4.name" equals "Debug.LogError"
    And the event "breadcrumbs.4.type" equals "log"

    And the event "breadcrumbs.5.name" equals "Exception"
    And the event "breadcrumbs.5.type" equals "error"
    And the event "breadcrumbs.5.metaData.message" equals "Error1"

    And the event "breadcrumbs.6.name" equals "Exception"
    And the event "breadcrumbs.6.type" equals "error"
    And the event "breadcrumbs.6.metaData.message" equals "Error2"

    And the event "breadcrumbs.7.name" equals "Metadata"
    And the event "breadcrumbs.7.type" equals "manual"
    And the event "breadcrumbs.7.metaData.test" equals "value"
    And the event "breadcrumbs.7.metaData.nullTest" is null



