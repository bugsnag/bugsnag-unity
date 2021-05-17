Feature: Android smoke tests for sessions

    Background:
        Given I wait for the game to start

    Scenario: Bugsnag automatically tracks sessions
        When I tap the "Notify with callback" button
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
        And the session payload field "app.durationInForeground" is not null
        And the session payload field "app.memoryUsage" is not null
        And the session payload field "app.isLaunching" equals "true"
        And the session payload field "app.type" equals "android"
        And the session payload field "app.duration" is not null
        And the session payload field "app.inForeground" equals "true"
        And the session payload field "app.activeScreen" equals "UnityPlayerActivity"
        And the session payload field "app.name" equals "maze_runner"
        And the session payload field "app.id" equals "com.bugsnag.mazerunner"
        And the session payload field "app.lowMemory" equals "false"

        # Device data
        And the session payload field "device.cpuAbi" is a non-empty array
        And the session payload field "device.jailbroken" equals "false"
        And the session payload field "device.id" is not null
        And the session payload field "device.id" is stored as the value "device_id"
        And the session payload field "device.locale" is not null
        And the session payload field "device.manufacturer" is not null
        And the session payload field "device.model" is not null
        And the session payload field "device.osName" equals "android"
        And the session payload field "device.osVersion" is not null
        And the session payload field "device.runtimeVersions" is not null
        And the session payload field "device.totalMemory" is not null
        And the session payload field "device.freeDisk" is not null
        And the session payload field "device.freeMemory" is not null
        And the session payload field "device.orientation" equals "portrait"
        And the session payload field "device.time" is not null
        And the session payload field "device.locationStatus" is not null
        And the session payload field "device.emulator" equals "false"
        And the session payload field "device.networkAccess" is not null
        And the session payload field "device.charging" is not null
        And the session payload field "device.screenDensity" is not null
        And the session payload field "device.dpi" is not null
        And the session payload field "device.screenResolution" is not null
        And the session payload field "device.brand" is not null
        And the session payload field "device.batteryLevel" is not null

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

    # Scenario: JVM errors contain session information
    # TODO receive and validate sessions in a JVM payload here
