When("I build a Unity application") do
  run_required_commands([
    ["features/scripts/create_unity_project.sh"]
  ])
end

When("I run the application") do
  steps %Q{
    When I set environment variable "MAZE_ENDPOINT" to "http://localhost:#{MOCK_API_PORT}"
  }
  run_required_commands([
    ["features/scripts/launch_unity_application.sh"]
  ])
end

When("I run the game in the {string} state") do |state|
  steps %Q{
    When I set environment variable "BUGSNAG_SCENARIO" to "#{state}"
    When I run the application
  }
end
