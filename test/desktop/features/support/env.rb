require 'fileutils'

$api_key = 'a35a2a72bd230ac0aa0f52715bbdc6aa'

Maze.hooks.before do
  if Maze.config.os == 'macos'
    $logger.info 'Clearing '
    FileUtils.rm_rf('~/Library/Application Support/com.bugsnag.Bugsnag')
  end
end

AfterConfiguration do |_config|
  Maze.config.enforce_bugsnag_integrity = false

  if Maze.config.os == 'macos'
    # The default macOS Crash Reporter "#{app_name} quit unexpectedly" alert grabs focus which can cause tests to flake.
    # This option, which appears to have been introduced in macOS 10.11, displays a notification instead of the alert.
    Maze::Runner.run_command('defaults write com.apple.CrashReporter UseUNC 1')
  end
end

at_exit do
  if Maze.config.os == 'macos'
    Maze::Runner.run_command("log show --predicate '(process == \"#{Maze.config.app}\")' --style syslog --start '#{Maze.start_time}' > #{Maze.config.app}.log")
  end
end
