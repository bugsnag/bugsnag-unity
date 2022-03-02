Feature: Unity Persistence

    @skip_webgl
    Scenario: Receive a persisted session mac and windows
        When I run the game in the "PersistSession" state
        And I wait for 5 seconds
        And I run the game in the "PersistSessionReport" state
        And I wait to receive 2 sessions
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the session payload field "app.releaseStage" equals the platform-dependent string:
            | macos | Second Session |
            | windows | First Session |
        And I discard the oldest session
        And the session payload field "app.releaseStage" equals the platform-dependent string:
            | macos | First Session |
            | windows | Second Session |
        And I run the game in the "ClearBugsnagCache" state
        And I wait for 5 seconds


    @webgl_only
    Scenario: Receive a persisted session webgl
        When I run the game in the "PersistSession" state
        And I wait for 5 seconds
        And I run the game in the "PersistSessionReport" state
        And I wait to receive 2 sessions
        Then the session is valid for the session reporting API version "1.0" for the "Unity Bugsnag Notifier" notifier
        And the session payload field "app.releaseStage" equals "First Session"
        And I discard the oldest session
        And the session payload field "app.releaseStage" equals "Second Session"
        And I run the game in the "ClearBugsnagCache" state
        And I wait for 5 seconds
    
    Scenario: Receive a persisted event
        When I run the game in the "PersistEvent" state
        And I wait for 5 seconds
        And I run the game in the "PersistEventReport" state
        And I wait to receive 2 errors
        And the event "context" equals "First Error"
        And the exception "message" equals "First Event"
        And I discard the oldest error
        And the event "context" equals "Second Error"
        And the exception "message" equals "Second Event"
        And I run the game in the "ClearBugsnagCache" state
        And I wait for 5 seconds

    Scenario: Receive a persisted event with on send callback
        When I run the game in the "PersistEvent" state
        And I wait for 5 seconds
        And I run the game in the "PersistEventReportCallback" state
        And I wait to receive 2 errors
        And the event "context" equals "First Error"
        And the exception "message" equals "First Event"

        #device metadata
        And the event "device.id" equals "Persist Id"

        #app metadata
        And the event "app.binaryArch" equals "Persist BinaryArch"
 
        #exception data
        And the event "exceptions.0.errorClass" equals "Persist ErrorClass"
        And the event "exceptions.0.stacktrace.0.method" equals "Persist Method"

        #breadcrumbs
        And the event "breadcrumbs.0.name" equals "Persist Message"

        #metadata
        And the event "metaData.Persist Section.Persist Key" equals "Persist Value"
        And I run the game in the "ClearBugsnagCache" state
        And I wait for 5 seconds

    Scenario: Persist Device Id
        When I run the game in the "PersistDeviceId" state
        And I wait to receive an error
        And the exception "message" equals "PersistDeviceId"
        And the error payload field "events.0.device.id" is stored as the value "device_id"
        And the error payload field "events.0.user.id" equals the stored value "device_id"
        And I discard the oldest error
        And I wait for 5 seconds
        And I run the game in the "PersistDeviceId" state
        And I wait to receive an error
        And the exception "message" equals "PersistDeviceId"   
        And the error payload field "events.0.device.id" equals the stored value "device_id"
        And the error payload field "events.0.user.id" equals the stored value "device_id"
        And I run the game in the "ClearBugsnagCache" state
        And I wait for 5 seconds
        