#!/bin/sh -ue

brew cask install \
  mono-mdk \
  android-sdk \
  bugsnag/unity/$UNITY_VERSION

export PATH="$PATH:/Library/Frameworks/Mono.framework/Versions/Current/Commands"

yes | sdkmanager "platforms;android-27"
yes | sdkmanager --licenses

bundle exec rake travis:export_plugin

# copy it to the directory that is being synchronised with S3
cp Bugsnag.unitypackage ~/$TRAVIS_BUILD_NUMBER
