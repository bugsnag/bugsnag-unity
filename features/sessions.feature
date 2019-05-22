Feature: Session Tracking

    Scenario: Automatically receiving a session
        When I run the game in the "AutoSession" state
        Then I should receive a request
        And the request is a valid for the session tracking API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "sessions" is an array with 1 element
        And the payload field "app.version" is not null
        And the payload field "app.releaseStage" equals "production"
        And the payload field "app.type" equals "Mac OS"
        And the payload field "device.osVersion" is not null
        And the payload field "device.osName" equals "Mac OS"
        And the payload field "device.model" is not null
        And the payload field "device.manufacturer" equals "Apple"

        And the session "id" is not null
        And the session "startedAt" is not null
        And the session "user.id" is not null
        And the session "user.email" is null
        And the session "user.name" is null

    Scenario: Automatically receiving a session before a native crash
        When I run the game in the "AutoSessionNativeCrash" state
        And I run the game in the "(noop)" state
        Then I should receive 2 requests
        And request 0 is valid for the session tracking API
        And request 1 is valid for the error reporting API
        And the payload field "events" is an array with 1 element for request 1
        And the payload field "events.0.session.events.handled" equals 0 for request 1
        And the payload field "events.0.session.events.unhandled" equals 1 for request 1
        And the payload field "events.0.session.id" of request 1 equals the payload field "sessions.0.id" of request 0

    Scenario: Manually logging a session
        When I run the game in the "ManualSession" state
        Then I should receive a request
        And the request is a valid for the session tracking API
        And the "Bugsnag-API-Key" header equals "a35a2a72bd230ac0aa0f52715bbdc6aa"
        And the payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the payload field "sessions" is an array with 1 element
        And the payload field "app.version" is not null
        And the payload field "app.releaseStage" equals "production"
        And the payload field "app.type" equals "Mac OS"
        And the payload field "device.osVersion" is not null
        And the payload field "device.osName" equals "Mac OS"
        And the payload field "device.model" is not null
        And the payload field "device.manufacturer" equals "Apple"

        And the session "id" is not null
        And the session "startedAt" is not null
        And the session "user.id" is not null
        And the session "user.email" is null
        And the session "user.name" is null

    Scenario: Manually logging a session before unhandled event
        When I run the game in the "ManualSessionCrash" state
        Then I should receive 2 requests
        And request 0 is valid for the session tracking API
        And request 1 is valid for the error reporting API
        And the payload field "events" is an array with 1 element for request 1
        And the payload field "events.0.session.events.handled" equals 0 for request 1
        And the payload field "events.0.session.events.unhandled" equals 1 for request 1
        And the payload field "events.0.session.id" of request 1 equals the payload field "sessions.0.id" of request 0

    Scenario: Manually logging a session before handled events
        When I run the game in the "ManualSessionNotify" state
        Then I should receive 2 requests
        And request 0 is valid for the session tracking API
        And request 1 is valid for the error reporting API
        And the payload field "events" is an array with 1 element for request 1
        And the payload field "events.0.session.events.handled" equals 1 for request 1
        And the payload field "events.0.session.events.unhandled" equals 0 for request 1
        And the payload field "events.0.session.id" of request 1 equals the payload field "sessions.0.id" of request 0

    Scenario: Manually logging a session before different types of events
        When I run the game in the "ManualSessionMixedEvents" state
        Then I should receive 4 requests
        And request 0 is valid for the session tracking API
        And request 1 is valid for the error reporting API
        And request 2 is valid for the error reporting API
        And request 3 is valid for the error reporting API
        And the events in requests "1,2,3" match one of:
            | message                      | handled | unhandled |
            | blorb                        | 1       | 0         |
            | Something went terribly awry | 2       | 0         |
            | Invariant state failure      | 2       | 1         |
