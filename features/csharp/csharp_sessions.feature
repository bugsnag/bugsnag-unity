Feature: Session Tracking

Scenario Outline: Automatically receiving a session
    When I run the game in the "StartSDKDefault" state
    And I wait to receive a session
    Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
    And the session payload field "app.version" is not null
    And the session payload field "app.releaseStage" equals "production"
    And the session payload field "app.type" equals the platform-dependent string:
      | macos   | MacOS   |
      | windows | Windows |
      | switch | nintendo-switch |
    # TODO Reported as 0.0.0.0 on our SDEV
    And the session payload field "device.osVersion" is not null
    And the session payload field "device.osName" equals the platform-dependent string:
      | macos   | Mac OS               |
      | windows | Microsoft Windows NT |
      | switch | Nintendo Switch |
    And the session payload field "device.model" is not null
    And the session payload field "device.manufacturer" equals the platform-dependent string:
      | macos   | Apple |
      | windows | PC    |
      | switch | Nintendo |
    And the session "id" is not null
    And the session "startedAt" is not null
    And the session "user.id" is not null
    And the session "user.email" is null
    And the session "user.name" is null


  # @macos_only
  # Scenario: Automatically receiving a session before a native crash
  #   When I run the game in the "AutoSessionNativeCrash" state
  #   And I wait for 5 seconds
  #   And I run the game in the "(noop)" state
  #   And I wait to receive a session
  #   And I wait to receive an error
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the error is valid for the error reporting API sent by the native Unity notifier
  #   And the event "session.events.handled" equals 0
  #   And the event "session.events.unhandled" equals 1
  #   And the error payload field "events.0.session.id" is stored as the value "session_id"
  #   And the session payload field "sessions.0.id" equals the stored value "session_id"

  # @skip_webgl
  # Scenario Outline: Manually logging a session
  #   When I run the game in the "<scenario>" state
  #   And I wait to receive a session
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the session payload field "app.version" is not null
  #   And the session payload field "app.releaseStage" equals "production"
  #   # TODO On our Switch SDEV this is 0.0.0.0
  #   And the session payload field "device.osVersion" is not null
  #   And the session payload field "device.osName" equals the platform-dependent string:
  #     | macos   | Mac OS               |
  #     | windows | Microsoft Windows NT |
  #     | switch | Nintendo Switch |
  #   And the session payload field "device.model" is not null
  #   And the session payload field "device.manufacturer" equals the platform-dependent string:
  #     | macos   | Apple |
  #     | windows | PC    |
  #     | switch | Nintendo |

  #   And the session "id" is not null
  #   And the session "startedAt" is not null
  #   And the session "user.id" is not null
  #   And the session "user.email" is null
  #   And the session "user.name" is null

  #   Examples:
  #     | scenario                           |
  #     | ManualSession                      |
  #     | ManualSessionInNotifyReleaseStages |

  # @webgl_only
  # Scenario Outline: Manually logging a session
  #   When I run the game in the "<scenario>" state
  #   And I wait to receive a session
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the session payload field "app.version" is not null
  #   And the session payload field "app.releaseStage" equals "production"
  #   And the session payload field "app.type" equals "WebGL"
  #   And the session payload field "device.osVersion" is not null
  #   And the session payload field "device.osName" equals "Unix"
  #   And the session payload field "device.model" is not null
  #   And the session payload field "device.manufacturer" is null
  #   And the session "id" is not null
  #   And the session "startedAt" is not null
  #   And the session "user.id" is not null
  #   And the session "user.email" is null
  #   And the session "user.name" is null

  #   Examples:
  #     | scenario                           |
  #     | ManualSession                      |
  #     | ManualSessionInNotifyReleaseStages |

  # Scenario: Manually logging a session before unhandled event
  #   When I run the game in the "ManualSessionCrash" state
  #   And I wait to receive a session
  #   And I wait to receive an error
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the event "session.events.handled" equals 0
  #   And the event "session.events.unhandled" equals 1
  #   And the error payload field "events.0.session.id" is stored as the value "session_id"
  #   And the session payload field "sessions.0.id" equals the stored value "session_id" ignoring case

  # Scenario: Manually logging a session before handled events
  #   When I run the game in the "ManualSessionNotify" state
  #   And I wait to receive a session
  #   And I wait to receive an error
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the event "session.events.handled" equals 1
  #   And the event "session.events.unhandled" equals 0
  #   And the error payload field "events.0.session.id" is stored as the value "session_id"
  #   And the session payload field "sessions.0.id" equals the stored value "session_id" ignoring case

  # Scenario: Manually logging a session before different types of events
  #   When I run the game in the "ManualSessionMixedEvents" state
  #   And I wait to receive a session
  #   And I wait to receive 3 errors
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the current error request events match one of:
  #     | message                      | handled | unhandled |
  #     | blorb                        | 1       | 0         |
  #     | Something went terribly awry | 2       | 0         |
  #     | Invariant state failure      | 2       | 1         |
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And I discard the oldest error
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And I discard the oldest error
  #   And the error is valid for the error reporting API sent by the Unity notifier

  # Scenario Outline: Launching the app but the current release stage is not in "notify release stages"
  #   When I run the game in the "<scenario>" state
  #   And I wait for 5 seconds
  #   Then I should receive no sessions

  #   Examples:
  #     | scenario                              |
  #     | ManualSessionNotInNotifyReleaseStages |
  #     | AutoSessionNotInNotifyReleaseStages   |

  #  Scenario: When a session is stopped the error has no session information
  #   When I run the game in the "StoppedSession" state
  #   And I wait to receive a session
  #   And I wait to receive an error
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the event "session" is null

  # Scenario: When a session is resumed the error uses the previous session information
  #   When I run the game in the "ResumedSession" state
  #   And I wait to receive a session
  #   And I wait to receive 2 errors
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the current error request events match one of:
  #     | message      | handled | unhandled |
  #     | First Error  | 1       | 0         |
  #     | Second Error | 2       | 0         |
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the error payload field "session.id" is stored as the value "session_id"
  #   And the error payload field "session.startedAt" is stored as the value "session_startedAt"
  #   And I discard the oldest error
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the error payload field "session.id" equals the stored value "session_id"
  #   And the error payload field "session.startedAt" equals the stored value "session_startedAt"

  # Scenario: When a new session is started the error uses different session information
  #   When I run the game in the "NewSession" state
  #   And I wait to receive 2 sessions
  #   And I wait to receive 2 errors
  #   # Session 1
  #   Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the session payload field "sessions.0.id" is stored as the value "session_id_1"
  #   And I discard the oldest session
  #   # Session 2
  #   And the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
  #   And the session payload field "sessions.0.id" does not equal the stored value "session_id_1"
  #   And the session payload field "sessions.0.id" is stored as the value "session_id_2"
  #   # Error 1
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the event "session.events.handled" equals 1
  #   And the error payload field "events.0.session.id" equals the stored value "session_id_1" ignoring case
  #   And I discard the oldest error
  #   # Error 2
  #   And the error is valid for the error reporting API sent by the Unity notifier
  #   And the event "session.events.handled" equals 1
  #   And the error payload field "events.0.session.id" equals the stored value "session_id_2" ignoring case
  #   And the error payload field "events.0.session.id" does not equal the stored value "session_id_1"
