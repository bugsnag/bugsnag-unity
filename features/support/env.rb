# Any 'run once' setup should go here as this file is evaluated
# when the environment loads.
# Any helper functions added here will be available in step
# definitions

# Scenario hooks
Before do
# Runs before every Scenario
end

After do
# Runs after every Scenario
end

at_exit do
  FileUtils.rm_rf('features/fixtures/Mazerunner.app')
  FileUtils.rm_rf('features/fixtures/unity_project')
end
