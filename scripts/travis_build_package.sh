#!/bin/sh -ue

brew install ninja
brew cask install \
  mono-mdk \
  android-sdk \
  bugsnag/unity/$UNITY_VERSION

export PATH="$PATH:/Library/Frameworks/Mono.framework/Versions/Current/Commands"

yes | sdkmanager "platforms;android-27" > /dev/null
yes | sdkmanager --licenses > /dev/null

curl --silent -o ndk.zip https://dl.google.com/android/repository/android-ndk-r16b-darwin-x86_64.zip
unzip -qq ndk.zip > /dev/null
mv android-ndk-r16b $ANDROID_NDK_HOME
rm ndk.zip

bundle exec rake travis:export_plugin

# copy it to the directory that is being synchronised with S3
cp Bugsnag.unitypackage $TRAVIS_BUILD_DIR/build_artifacts
cp Bugsnag-with-android-64bit.unitypackage $TRAVIS_BUILD_DIR/build_artifacts
