#!/bin/sh -uex

# installs three versions of Unity that we want to maintain compatibility across
# moves each version to a separate versioned directory within `/Applications`
# and links the 2018 version to `/Applications/Unity`

brew tap caskroom/cask
brew tap bugsnag/unity

for version in 2018.4.36f1 2019.4.29f1 2020.3.15f2 2021.1.16f1
do
  brew cask install \
    unity-$version \
    unity-ios-support-for-editor-$version \
    unity-android-support-for-editor-$version \
    unity-tvos-support-for-editor-$version

  mv /Applications/Unity/ /Applications/Unity-$version/
done

ln -s /Applications/Unity-2018.4.36f1 /Applications/Unity
