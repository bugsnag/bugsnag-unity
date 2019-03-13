Feature: Stopping and resuming sessions

Scenario: When a session is stopped the error has no session information
    When I run the game in the "StoppedSession" state
    Then I should receive 2 requests
    And the request 0 is valid for the session tracking API
    And the request 1 is valid for the error reporting API
    And the payload field "events.0.session" is null for request 1

Scenario: When a session is resumed the error uses the previous session information
    When I run the game in the "ResumedSession" state
    Then I should receive 3 requests
    And the request 0 is valid for the session tracking API
    And the request 1 is valid for the error reporting API
    And the request 2 is valid for the error reporting API
    And the payload field "events.0.session.id" of request 1 equals the payload field "events.0.session.id" of request 2
    And the payload field "events.0.session.startedAt" of request 1 equals the payload field "events.0.session.startedAt" of request 2
    And the events in requests "1,2" match one of:
        | message      | handled | unhandled |
        | First Error  | 1       | 0         |
        | Second Error | 2       | 0         |

Scenario: When a new session is started the error uses different session information
    When I run the game in the "NewSession" state
    Then I should receive 4 requests
    And the request 0 is valid for the session tracking API
    And the request 1 is valid for the session tracking API
    And the request 2 is valid for the error reporting API
    And the request 3 is valid for the error reporting API
    And the payload field "events.0.session.events.handled" equals 1 for request 2
    And the payload field "events.0.session.events.handled" equals 1 for request 3
    And the payload field "events.0.session.id" of request 2 does not equal the payload field "events.0.session.id" of request 3
