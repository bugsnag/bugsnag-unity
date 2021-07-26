Feature: iOS smoke tests for sessions

    # TODO: Failing steps commented out pending full implementation of PLAT-6372
    Background:
        Given I wait for the mobile game to start

    Scenario: Bugsnag automatically tracks sessions
        When I run the "Notify with callback" mobile scenario
        And I wait to receive a session

        # Session payload
        And the session payload field "notifier.name" equals "Unity Bugsnag Notifier"
        And the session payload field "sessions" is an array with 1 elements
        And the session "user.id" is not null
        And the session payload field "sessions.0.user.id" is stored as the value "automated_user_id"
        And the session "id" is not null
        And the session payload field "sessions.0.id" is stored as the value "automated_session_id"
        And the session "startedAt" is not null

        # App data
        And the session payload field "app.version" equals "1.2.3"
        And the session payload field "app.releaseStage" equals "production"
        And the session payload field "app.bundleVersion" is not null
        And the session payload field "app.id" equals "com.bugsnag.unity.mazerunner"
        And the session payload field "app.type" equals "iOS"

        # Device data
        And the session payload field "device.locale" is not null
        And the session payload field "device.timezone" is not null
        And the session payload field "device.osName" equals "iOS"
        And the session payload field "device.time" is not null
        And the session payload field "device.osVersion" is not null
        And the session payload field "device.runtimeVersions" is not null
        And the session payload field "device.osVersion" is not null
        And the session payload field "device.freeDisk" is not null
        And the session payload field "device.freeMemory" is not null
        And the session payload field "device.id" is not null
        And the session payload field "device.jailbroken" is not null
        And the session payload field "device.manufacturer" equals "Apple"
        And the session payload field "device.model" is not null
        And the session payload field "device.modelNumber" is not null

        # Session info is included in Error payload
        Then I wait to receive an error
        And the error payload field "events" is an array with 1 elements
        And the exception "errorClass" equals "ExecutionEngineException"
        And the exception "message" equals "This one has a callback"
        And the event "user.id" is not null
        And the event "device.id" is not null
        And the error payload field "events.0.user.id" equals the stored value "automated_user_id"
        And the error payload field "events.0.device.id" equals the stored value "automated_user_id"
        And the event "session.id" is not null
        And the error payload field "events.0.session.id" equals the stored value "automated_session_id"
        And the event "session.events.handled" equals 1
        And the event "session.events.unhandled" equals 0
