Feature: Csharp Breadcrumbs

  Background:
    Given I clear the Bugsnag cache

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

  @skip_webgl
  Scenario: Breadcrumb Truncation
    When I run the game in the "BreadcrumbTruncation" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "BreadcrumbTruncation"
    And the error payload field "events.0.breadcrumbs" is an array with 99 elements
    And the event "breadcrumbs.98.name" equals "Removed, along with 2 older breadcrumbs, to reduce payload size"
    And the event "breadcrumbs.98.type" equals "manual"

 Scenario: Network breadcrumb success
    When I run the game in the "NetworkBreadcrumbsSuccess" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "NetworkBreadcrumbsSuccess"
    
    And the event "breadcrumbs.1.name" equals "UnityWebRequest succeeded"
    And the event "breadcrumbs.1.type" equals "request"
    And the event "breadcrumbs.1.metaData.method" equals "GET" 
    And the error payload field "events.0.breadcrumbs.1.metaData.url" matches the regex "^http:\/\/\S*:\d{4}(\/.*)?"
    And the event "breadcrumbs.1.metaData.status" equals 200
    And the event "breadcrumbs.1.metaData.urlParams.success" equals "true"
    And the event "breadcrumbs.1.metaData.urlParams.redactthis" equals "[REDACTED]"
    And the event "breadcrumbs.1.metaData.duration" is greater than 0
    And the event "breadcrumbs.1.metaData.responseContentLength" is greater than 0

    And the event "breadcrumbs.2.name" equals "UnityWebRequest succeeded"
    And the event "breadcrumbs.2.type" equals "request"
    And the event "breadcrumbs.2.metaData.method" equals "POST" 
    And the error payload field "events.0.breadcrumbs.2.metaData.url" matches the regex "^http:\/\/\S*:\d{4}(\/.*)?"
    And the event "breadcrumbs.2.metaData.status" equals 200
    And the event "breadcrumbs.2.metaData.duration" is greater than 0
    And the event "breadcrumbs.2.metaData.responseContentLength" is greater than 0
    And the event "breadcrumbs.2.metaData.requestContentLength" is greater than 0


 Scenario: Network breadcrumb fails
    When I run the game in the "NetworkBreadcrumbsFail" state
    And I wait to receive an error
    Then the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "NetworkBreadcrumbsFail"
    And the event "breadcrumbs.1.name" equals "UnityWebRequest failed"
    And the event "breadcrumbs.1.type" equals "request"
    And the event "breadcrumbs.1.metaData.method" equals "GET" 
    And the error payload field "events.0.breadcrumbs.1.metaData.url" equals "https://localhost:994/?success=false"
    And the event "breadcrumbs.1.metaData.status" equals 0
    And the event "breadcrumbs.1.metaData.urlParams.success" equals "false"
    And the event "breadcrumbs.1.metaData.duration" is greater than 0

