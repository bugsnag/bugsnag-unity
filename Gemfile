source 'https://rubygems.org'

gem 'rake'
gem 'xcpretty'
gem 'xcodeproj'

unless Gem.win_platform?
  # Use official Maze Runner release
  gem 'bugsnag-maze-runner', git: 'https://github.com/bugsnag/maze-runner', tag: 'v6.18.1'

  # Use a specific Maze Runner branch
  #gem 'bugsnag-maze-runner', git: 'https://github.com/bugsnag/maze-runner', branch: 'master'

  # Use a local copy of Maze Runner for development purposes
  #gem 'bugsnag-maze-runner', path: '../maze-runner'
end
