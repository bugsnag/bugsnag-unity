# Any 'run once' setup should go here as this file is evaluated
# when the environment loads.
# Any helper functions added here will be available in step
# definitions

ENV['BUGSNAG_APIKEY'] = 'a35a2a72bd230ac0aa0f52715bbdc6aa'
ENV['UNITY_TEST_PROJECT'] = "features/fixtures/Mazerunner-#{ENV['UNITY_VERSION']}.app"

# Scenario hooks
Before do
# Runs before every Scenario
end

After do
# Runs after every Scenario
end

at_exit do
  `pkill Mazerunner`
  FileUtils.rm_rf(ENV['UNITY_TEST_PROJECT'])
end
