Feature: Stopping and resuming sessions

Scenario: When a session is stopped the error has no session information
    When I run the game in the "StoppedSession" state
    And I wait to receive a session
    And I wait to receive an error
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
    And the event "session" is null

Scenario: When a session is resumed the error uses the previous session information
    When I run the game in the "ResumedSession" state
    And I wait to receive a session
    And I wait to receive 2 errors
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the current error request events match one of:
        | message      | handled | unhandled |
        | First Error  | 1       | 0         |
        | Second Error | 2       | 0         |
    And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
    And the error payload field "session.id" is stored as the value "session_id"
    And the error payload field "session.startedAt" is stored as the value "session_startedAt"
    And I discard the oldest error
    And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
    And the error payload field "session.id" equals the stored value "session_id"
    And the error payload field "session.startedAt" equals the stored value "session_startedAt"

Scenario: When a new session is started the error uses different session information
    When I run the game in the "NewSession" state
    And I wait to receive 2 sessions
    And I wait to receive 2 errors
    # Session 1
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "sessions.0.id" is stored as the value "session_id_1"
    And I discard the oldest session
    # Session 2
    And the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "sessions.0.id" does not equal the stored value "session_id_1"
    And the session payload field "sessions.0.id" is stored as the value "session_id_2"
    # Error 1
    And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
    And the event "session.events.handled" equals 1
    And the error payload field "events.0.session.id" equals the stored value "session_id_1"
    And I discard the oldest error
    # Error 2
    And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
    And the event "session.events.handled" equals 1
    And the error payload field "events.0.session.id" equals the stored value "session_id_2"
    And the error payload field "events.0.session.id" does not equal the stored value "session_id_1"
