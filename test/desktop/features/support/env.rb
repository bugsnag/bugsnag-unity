require 'fileutils'

$api_key = 'a35a2a72bd230ac0aa0f52715bbdc6aa'


AfterConfiguration do |_config|
  raise '--os option must be set (to "macos" or "windows"' if Maze.config.os.nil?

  Maze.config.enforce_bugsnag_integrity = false

  if Maze.config.os.downcase == 'macos'
    # The default macOS Crash Reporter "#{app_name} quit unexpectedly" alert grabs focus which can cause tests to flake.
    # This option, which appears to have been introduced in macOS 10.11, displays a notification instead of the alert.
    Maze::Runner.run_command('defaults write com.apple.CrashReporter UseUNC 1')
  end
end

Maze.hooks.before do
  if Maze.config.os == 'macos'
    support_dir = File.expand_path '~/Library/Application Support/com.bugsnag.Bugsnag'
    $logger.info 'Clearing #{support_dir}'
    FileUtils.rm_rf(support_dir)
  end
end

at_exit do
  if Maze.config.os == 'macos'
    app_name = Maze.config.app.gsub /\.app$/, ''
    Maze::Runner.run_command("log show --predicate '(process == \"#{app_name}\")' --style syslog --start '#{Maze.start_time}' > #{app_name}.log")
  end
end
