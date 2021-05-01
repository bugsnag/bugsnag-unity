Feature: Android manual smoke tests

  Scenario: Throw exception
    When I tap the "throw Exception" button
    Then I wait to receive an error
