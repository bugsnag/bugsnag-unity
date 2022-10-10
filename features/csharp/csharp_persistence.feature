Feature: Unity Persistence

  Scenario: Receive a persisted session mac and windows
    When I clear the Bugsnag cache
    And I wait for 3 seconds
    And I close the Unity app
    And I run the game in the "PersistSession" state
    And I wait for 5 seconds
    And I close the Unity app
    And I run the game in the "PersistSessionReport" state
    And I wait to receive 2 sessions
    And I sort the sessions by the payload field "app.releaseStage"
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "app.releaseStage" equals "Session 1"
    And I discard the oldest session
    And the session payload field "app.releaseStage" equals "Session 2"

  Scenario: Receive a persisted event
    When I clear the Bugsnag cache
    And I wait for 3 seconds
    And I close the Unity app
    And I run the game in the "PersistEvent" state
    And I wait for 5 seconds
    And I close the Unity app
    And I run the game in the "PersistEventReport" state
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    And the event "context" equals "Error 1"
    And the exception "message" equals "Error 1"
    And I discard the oldest error
    And the event "context" equals "Error 2"
    And the exception "message" equals "Error 2"

  Scenario: Receive a persisted event with on send callback
    When I clear the Bugsnag cache
    And I wait for 3 seconds
    And I close the Unity app
    And I run the game in the "PersistEvent" state
    And I wait for 5 seconds
    And I close the Unity app
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

  # Scenario: Persist Device Id
  #   When I clear the Bugsnag cache
  #   And I wait for 5 seconds
  #   And I close the Unity app
  #   And I run the game in the "PersistDeviceId" state
  #   And I wait to receive an error
  #   And the exception "message" equals "PersistDeviceId"
  #   And the error payload field "events.0.device.id" is stored as the value "device_id"
  #   And the error payload field "events.0.user.id" equals the stored value "device_id"
  #   And I discard the oldest error

  #   And I wait for 5 seconds
  #   And I close the Unity app
  #   And I run the game in the "PersistDeviceId" state
  #   And I wait to receive an error
  #   And the exception "message" equals "PersistDeviceId"
  #   And the error payload field "events.0.device.id" equals the stored value "device_id"
  #   And the error payload field "events.0.user.id" equals the stored value "device_id"

  # # TODO: Currently failing on WebGL
  # @skip_webgl
  # Scenario: Max Persisted Events
  #   When I clear the Bugsnag cache
  #   And I wait for 5 seconds
  #   And I close the Unity app
  #   And I run the game in the "MaxPersistEvents" state
  #   And I wait for 20 seconds
  #   And I close the Unity app
  #   And I run the game in the "(noop)" state
  #   And I wait for 5 seconds
  #   And I wait to receive 4 errors
  #   And the exception "message" equals "Event 1"
  #   And I discard the oldest error
  #   And the exception "message" equals "Event 2"
  #   And I discard the oldest error
  #   And the exception "message" equals "Event 3"
  #   And I discard the oldest error
  #   And the exception "message" equals "Event 4"
