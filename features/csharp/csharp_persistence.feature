Feature: Unity Persistence

  Background:
    Given I clear the Bugsnag cache

  @skip_windows @skip_webgl @skip_cocoa @skip_android #pending PLAT-8632
  Scenario: Receive a persisted session
    When I set the HTTP status code for the next requests to "408"
    And I run the game in the "PersistSession" state
    And I wait to receive 1 session
    And I wait for requests to persist
    And I discard the oldest session
    And I close the Unity app
    And On Mobile I relaunch the app
    And I run the game in the "PersistSessionReport" state
    And I wait to receive 2 sessions
    And I sort the sessions by the payload field "app.releaseStage"
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "app.releaseStage" equals "Session 1"
    And I discard the oldest session
    And the session payload field "app.releaseStage" equals "Session 2"

  Scenario: Receive a persisted event
    When I set the HTTP status code for the next requests to "408"
    And I run the game in the "PersistEvent" state
    And I wait to receive an error
    And I wait for requests to persist
    And I discard the oldest error
    And I close the Unity app
    And On Mobile I relaunch the app
    And I run the game in the "PersistEventReport" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    And the error is valid for the error reporting API sent by the Unity notifier
    And the event "context" equals "Error 1"
    And the exception "message" equals "Error 1"
    And I discard the oldest error
    And the error is valid for the error reporting API sent by the Unity notifier
    And the event "context" equals "Error 2"
    And the exception "message" equals "Error 2"

  Scenario: Receive a persisted event with on send callback
    When I set the HTTP status code for the next requests to "408"
    And I run the game in the "PersistEvent" state
    And I wait to receive an error
    And I wait for requests to persist
    And I discard the oldest error
    And I close the Unity app
    And On Mobile I relaunch the app
    And I run the game in the "PersistEventReportCallback" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    And the event "context" equals "Error 1"
    And the exception "message" equals "Error 1"
    And the event "device.id" equals "Persist Id"
    And the event "app.binaryArch" equals "Persist BinaryArch"
    And the event "exceptions.0.errorClass" equals "Persist ErrorClass"
    And the event "exceptions.0.stacktrace.0.method" equals "Persist Method"
    And the event "breadcrumbs.0.name" equals "Persist Message"
    And the event "metaData.Persist Section.Persist Key" equals "Persist Value"

  Scenario: Max Persisted Events
    When I set the HTTP status code for the next requests to "408,408,408,408"
    When I run the game in the "MaxPersistEvents" state
    And I wait to receive 4 errors
    And I wait for requests to persist
    And I discard the oldest error
    And I discard the oldest error
    And I discard the oldest error
    And I discard the oldest error
    And I close the Unity app
    And On Mobile I relaunch the app
    And I run the game in the "ReportMaxPersistedEvents" state
    And I wait to receive an error
    And the exception "message" equals "true"

  @skip_cocoa @skip_android #These platforms handle sessions separately and will have separate tests
  Scenario: Max Persisted Sessions
    When I set the HTTP status code for the next requests to "408,408,408,408"
    And I run the game in the "MaxPersistSessions" state
    And I wait to receive 4 sessions
    And I wait for requests to persist
    And I discard the oldest session
    And I discard the oldest session
    And I discard the oldest session
    And I discard the oldest session
    And I close the Unity app
    And On Mobile I relaunch the app
    And I run the game in the "ReportMaxPersistedSessions" state
    And I wait to receive an error
    And the exception "message" equals "true"

  Scenario: Persist Device Id
    When I run the game in the "PersistDeviceId" state
    And I wait to receive an error
    And the exception "message" equals "PersistDeviceId"
    And the error payload field "events.0.device.id" is stored as the value "device_id"
    And I discard the oldest error
    And I close the Unity app
    And On Mobile I relaunch the app
    And I run the game in the "PersistDeviceId" state
    And I wait to receive an error
    And the exception "message" equals "PersistDeviceId"
    And the error payload field "events.0.device.id" equals the stored value "device_id"

