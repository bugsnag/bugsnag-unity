#!/bin/sh -uex

# installs three versions of Unity that we want to maintain compatibility across
# moves each version to a separate versioned directory within `/Applications`
# and links the 2017 version to `/Applications/Unity`

brew tap caskroom/cask
brew tap bugsnag/unity

for version in 2018-2-5f1 2017-4-1f1 5-6-6f2
do
  brew cask install \
    unity-$version \
    unity-ios-support-for-editor-$version \
    unity-android-support-for-editor-$version \
    unity-tvos-support-for-editor-$version

  mv /Applications/Unity/ /Applications/Unity-$version/
done

ln -s /Applications/Unity-2017-4-1f1 /Applications/Unity
