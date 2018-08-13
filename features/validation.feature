Feature: Ordering anything with lemon

Scenario: Lemon cake is in the online menu
  When I select "lemon cake" on the website
  Then I should receive a request
  And the payload body matches the JSON fixture in "features/fixtures/lemon.json"

Scenario: Lemon meringue is in the online menu
  When I select "lemon meringue" on the website
  Then I should receive a request
  And the payload body matches the JSON fixture in "features/fixtures/lemon.json"
