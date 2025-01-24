source 'https://rubygems.org'

gem 'rake'
gem 'xcpretty'
gem 'xcodeproj'
gem 'cocoapods'
gem 'danger'

unless Gem.win_platform?
  # Use official Maze Runner release
  # TODO Temporary workaround for uri 1.0.0 causing requests to fail
  gem 'uri', '0.13.1'
  gem 'bugsnag-maze-runner', '~>9.0'

  # Use a specific Maze Runner branch
  # gem 'bugsnag-maze-runner', git: 'https://github.com/bugsnag/maze-runner', branch: 'master'

  # Use a local copy of Maze Runner for development purposes
  #gem 'bugsnag-maze-runner', path: '../maze-runner'
end

# Only install bumpsnag if we're using Github actions
unless ENV['GITHUB_ACTIONS'].nil?
  gem 'bumpsnag', git: 'https://github.com/bugsnag/platforms-bumpsnag', branch: 'main'
end
