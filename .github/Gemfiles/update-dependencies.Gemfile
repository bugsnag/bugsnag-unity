source 'https://rubygems.org'

gem 'rake'
gem 'xcpretty'
gem 'xcodeproj'

# Only install bumpsnag if we're using Github actions
unless ENV['GITHUB_ACTIONS'].nil?
  gem 'bumpsnag', git: 'https://github.com/bugsnag/platforms-bumpsnag', branch: 'main'
end
