Feature: Session Tracking

  Background:
    Given I clear the Bugsnag cache

  Scenario: Unhandled error in session
    When I run the game in the "UnhandledErrorInSession" state
    And I wait to receive an error
    And I wait to receive a session
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "UnhandledErrorInSession"
    And the event "session.events.handled" equals 0
    And the event "session.events.unhandled" equals 1
    And the error payload field "events.0.session.id" is stored as the value "session_id"
    And the session payload field "sessions.0.id" equals the stored value "session_id" ignoring case

  Scenario: Handled error in session
    When I run the game in the "HandledErrorInSession" state
    And I wait to receive an error
    And I wait to receive a session
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "HandledErrorInSession"
    And the event "session.events.handled" equals 1
    And the event "session.events.unhandled" equals 0
    And the error payload field "events.0.session.id" is stored as the value "session_id"
    And the session payload field "sessions.0.id" equals the stored value "session_id" ignoring case

  @skip_macos #Unable to reliably get the in focus notification from macos
  Scenario: Automatically receiving a session
    When I run the game in the "StartSDKDefault" state
    And I wait to receive a session
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "app.version" is not null
    And the session payload field "app.releaseStage" equals "production"
    And the session payload field "app.type" equals the platform-dependent string:
      | macos   | MacOS           |
      | windows | Windows         |
      | switch  | nintendo-switch |
      | browser | WebGL           |
      | android | android         |
      | ios     | iOS             |
    And the session payload field "device.osVersion" is not null
    And the session payload field "device.osName" equals the platform-dependent string:
      | macos   | Mac OS               |
      | windows | Microsoft Windows NT |
      | switch  | Nintendo Switch      |
      | browser | Unix                 |
      | android | android              |
      | ios     | iOS                  |
    And the session payload field "device.model" is not null
    And the session payload field "device.manufacturer" equals the platform-dependent string:
      | macos   | Apple    |
      | windows | PC       |
      | switch  | Nintendo |
      | browser | @skip    |
      | android | @skip    |
      | ios     | Apple    |
    And the session "id" is not null
    And the session "startedAt" is not null
    And the session "user.id" is not null
    And the session "user.email" is null
    And the session "user.name" is null

  Scenario: Manually logging a session
    When I run the game in the "ManualSession" state
    And I wait to receive a session
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "app.version" is not null
    And the session payload field "app.releaseStage" equals "production"
    And the session payload field "app.type" equals the platform-dependent string:
      | macos   | MacOS           |
      | windows | Windows         |
      | switch  | nintendo-switch |
      | browser | WebGL           |
      | android | android         |
      | ios     | iOS             |
    And the session payload field "device.osVersion" is not null
    And the session payload field "device.osName" equals the platform-dependent string:
      | macos   | Mac OS               |
      | windows | Microsoft Windows NT |
      | switch  | Nintendo Switch      |
      | browser | Unix                 |
      | android | android              |
      | ios     | iOS                  |
    And the session payload field "device.model" is not null
    And the session payload field "device.manufacturer" equals the platform-dependent string:
      | macos   | Apple    |
      | windows | PC       |
      | switch  | Nintendo |
      | browser | @skip    |
      | android | @skip    |
      | ios     | Apple    |
    And the session "id" is not null
    And the session "startedAt" is not null
    And the session "user.id" is not null
    And the session "user.email" is null
    And the session "user.name" is null

  Scenario: Multiple event counts in one session
    When I run the game in the "MultipleEventCounts" state
    And I wait to receive a session
    And I wait to receive 3 errors
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the current error request events match one of:
      | message           | handled | unhandled |
      | Handled Error 1   | 1       | 0         |
      | Handled Error 2   | 2       | 0         |
      | Unhandled Error 1 | 2       | 1         |

  Scenario: No Auto session when not in enabled release stage
    When I run the game in the "SessionNotInReleaseStage" state
    And I wait for 5 seconds
    Then I should receive no sessions

  Scenario: When a session is Paused the error has no session information
    When I run the game in the "PausedSessionEvent" state
    And I wait to receive a session
    And I wait to receive an error
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "PausedSessionEvent"
    And the event "session" is null

  Scenario: When a session is resumed the error uses the previous session information
    When I run the game in the "ResumedSession" state
    And I wait to receive a session
    And I wait to receive 3 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the error is valid for the error reporting API sent by the Unity notifier
    And the exception "message" equals "Error 1"
    And the error payload field "session.id" is stored as the value "session_id"
    And the error payload field "session.startedAt" is stored as the value "session_startedAt"
    And the error payload field "events.0.session.events.handled" equals 1
    And I discard the oldest error
    And the exception "message" equals "Error 2"
    And the event "session" is null
    And I discard the oldest error
    And the exception "message" equals "Error 3"
    And the error is valid for the error reporting API sent by the Unity notifier
    And the error payload field "session.id" equals the stored value "session_id"
    And the error payload field "session.startedAt" equals the stored value "session_startedAt"
    And the error payload field "events.0.session.events.handled" equals 2

  Scenario: When a new session is started the error uses different session information
    When I run the game in the "NewSession" state
    And I wait to receive 2 sessions
    And I wait to receive 2 errors
    And I sort the errors by the payload field "events.0.exceptions.0.message"
    And I sort the sessions by the payload field "sessions.0.startedAt"
    # Session 1
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "sessions.0.id" is stored as the value "session_id_1"
    And I discard the oldest session
    # Session 2
    And the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "sessions.0.id" does not equal the stored value "session_id_1"
    And the session payload field "sessions.0.id" is stored as the value "session_id_2"
    # Error 1
    And the error is valid for the error reporting API sent by the Unity notifier
    And the event "session.events.handled" equals 1
    And the error payload field "events.0.session.id" equals the stored value "session_id_1" ignoring case
    And I discard the oldest error
    # Error 2
    And the error is valid for the error reporting API sent by the Unity notifier
    And the event "session.events.handled" equals 1
    And the error payload field "events.0.session.id" equals the stored value "session_id_2" ignoring case
