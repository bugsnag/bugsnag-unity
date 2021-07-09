Feature: Session Tracking

    Scenario Outline: Automatically receiving a session
        When I run the game in the "<scenario>" state
        And I wait to receive a session
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the session payload field "app.version" is not null
        And the session payload field "app.releaseStage" equals "production"
        And the session payload field "app.type" equals the platform-dependent string:
        | macos | Mac OS |
        | windows | Windows |
        And the session payload field "device.osVersion" is not null
        And the session payload field "device.osName" equals "Mac OS"
        And the session payload field "device.model" is not null
        And the session payload field "device.manufacturer" equals "Apple"
        And the session "id" is not null
        And the session "startedAt" is not null
        And the session "user.id" is not null
        And the session "user.email" is null
        And the session "user.name" is null
        Examples:
            | scenario                         |
            | AutoSession                      |
            | AutoSessionInNotifyReleaseStages |

    Scenario: Automatically receiving a session before a native crash
        When I run the game in the "AutoSessionNativeCrash" state
        And I run the game in the "(noop)" state
        And I wait to receive a session
        And I wait to receive an error
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the error is valid for the error reporting API sent by the platform-dependent string:
        | macos | OSX Bugsnag Notifier |
        | windows | Unity Bugsnag Notifier |


        And the event "session.events.handled" equals 0
        And the event "session.events.unhandled" equals 1
        And the error payload field "events.0.session.id" is stored as the value "session_id"
        And the session payload field "sessions.0.id" equals the stored value "session_id"

    Scenario Outline: Manually logging a session
        When I run the game in the "<scenario>" state
        And I wait to receive a session
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the session payload field "app.version" is not null
        And the session payload field "app.releaseStage" equals "production"
        And the session payload field "app.type" equals the platform-dependent string:
        | macos | Mac OS |
        | windows | Windows |
        And the session payload field "device.osVersion" is not null
        And the session payload field "device.osName" equals "Mac OS"
        And the session payload field "device.model" is not null
        And the session payload field "device.manufacturer" equals "Apple"

        And the session "id" is not null
        And the session "startedAt" is not null
        And the session "user.id" is not null
        And the session "user.email" is null
        And the session "user.name" is null

        Examples:
            | scenario                           |
            | ManualSession                      |
            | ManualSessionInNotifyReleaseStages |

    Scenario: Manually logging a session before unhandled event
        When I run the game in the "ManualSessionCrash" state
        And I wait to receive a session
        And I wait to receive an error
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the event "session.events.handled" equals 0
        And the event "session.events.unhandled" equals 1
        And the error payload field "events.0.session.id" is stored as the value "session_id"
        And the session payload field "sessions.0.id" equals the stored value "session_id"

    Scenario: Manually logging a session before handled events
        When I run the game in the "ManualSessionNotify" state
        And I wait to receive a session
        And I wait to receive an error
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the error is valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the event "session.events.handled" equals 1
        And the event "session.events.unhandled" equals 0
        And the error payload field "events.0.session.id" is stored as the value "session_id"
        And the session payload field "sessions.0.id" equals the stored value "session_id"

    Scenario: Manually logging a session before different types of events
        When I run the game in the "ManualSessionMixedEvents" state
        And I wait to receive a session
        And I wait to receive 3 errors
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the errors are valid for the error reporting API sent by the "Unity Bugsnag Notifier"
        And the current error request events match one of:
            | message                      | handled | unhandled |
            | blorb                        | 1       | 0         |
            | Something went terribly awry | 2       | 0         |
            | Invariant state failure      | 2       | 1         |

    Scenario Outline: Launching the app but the current release stage is not in "notify release stages"
        When I run the game in the "<scenario>" state
        And I wait for 5 seconds
        Then I should receive no sessions

        Examples:
            | scenario                              |
            | ManualSessionNotInNotifyReleaseStages |
            | AutoSessionNotInNotifyReleaseStages   |
