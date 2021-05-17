require 'fileutils'

$api_key = 'a35a2a72bd230ac0aa0f52715bbdc6aa'

AfterConfiguration do |_config|
  Maze.config.enforce_bugsnag_integrity = false

  project_name = Maze.config.app
  fixture_dir = 'features/fixtures'
  project_dir = "#{fixture_dir}/maze_runner"
  app_file = "#{project_name}.app"
  zip_file = "#{project_name}-#{ENV['UNITY_VERSION']}.app.zip"

  unless File.exist?("#{fixture_dir}/#{zip_file}")
    raise StandardError, "Test fixture build archive not found at #{fixture_dir}/#{zip_file}"
  end

  `cd #{fixture_dir} && tar -xzf #{zip_file}`

  unless File.exist?("#{project_dir}/#{app_file}")
    raise StandardError, "Test fixture wasn't successfully extracted to #{project_dir}/#{app_file}"
  end

  Maze::Runner.environment['UNITY_PROJECT_NAME'] = project_name

  at_exit do
    FileUtils.rm_rf(project_dir + app_file)
    Maze::Runner.run_command("log show --predicate '(process == \"#{Maze.config.app}\")' --style syslog --start '#{Maze.start_time}' > #{Maze.config.app}.log")
  end
end
