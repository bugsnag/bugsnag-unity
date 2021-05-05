# Any 'run once' setup should go here as this file is evaluated
# when the environment loads.
# Any helper functions added here will be available in step
# definitions

ENV['BUGSNAG_APIKEY'] = 'a35a2a72bd230ac0aa0f52715bbdc6aa'
unity_project_name = ENV['UNITY_PROJECT_NAME'] = "Mazerunner-#{ENV['UNITY_VERSION']}"
unity_test_project = "features/fixtures/#{unity_project_name}.app"

`cd features/fixtures && tar -xzf #{unity_project_name}.app.zip`

# Scenario hooks
Before do
# Runs before every Scenario
end

After do
# Runs after every Scenario
end

at_exit do
  `pkill Mazerunner`
  FileUtils.rm_rf(unity_test_project)
end
