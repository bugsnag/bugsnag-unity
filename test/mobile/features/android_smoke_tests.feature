Feature: Android manual smoke tests

  Background:
    Given I wait for the game to start

  Scenario: Press all the buttons
    When I tap the "throw Exception" button
    And I tap the "Assertion failure" button
    And I tap the "Native exception" button
    And I tap the "Log caught exception" button
    And I tap the "Log with class prefix" button
    And I tap the "Notify caught exception" button
    And I tap the "Notify with callback" button
    Then I wait to receive 7 errors
