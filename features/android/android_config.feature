Feature: Android Config

    Scenario: Android Persistence Directory
        When I run the game in the "AndroidPersistenceDirectory" state
        And I wait to receive an error
        And the exception "message" equals "Directory Found"



