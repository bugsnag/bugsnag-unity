Feature: android max breadcrumbs

        Background:
                Given I wait for the game to start

        Scenario: Max Breadcrumbs
                When I tap the "Max Breadcrumbs" button
                Then I wait to receive an error

                # Breadcrumbs
                And the error payload field "events.0.breadcrumbs" is an array with 5 elements
