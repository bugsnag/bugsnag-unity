Feature: test android breadcrumbs

        Background:
                Given I wait for the game to start

        Scenario: Uncaught C# exception
                When I tap the "Disable Breadcrumbs" button
                Then I wait to receive an error

                # Breadcrumbs
                And the error payload field "breadcrumbs.0" is null
