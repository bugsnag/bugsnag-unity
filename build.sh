#!/bin/bash
UNITY=/Applications/Unity/Unity.app/Contents/MacOS/Unity
EXEC_PATH=`pwd`
ANDROID_PATH="$EXEC_PATH/../bugsnag-android/"
TEMP_UNITY_PATH="$EXEC_PATH/temp.unityproject"
BUILD_PATH="$EXEC_PATH/build"

# Clean up
rm -rf $TEMP_UNITY_PATH
rm -rf $BUILD_PATH

mkdir -p $BUILD_PATH

echo "Creating new, empty unity3d project"
${UNITY} -batchmode -quit -createproject $TEMP_UNITY_PATH
if [ $? -ne 0 ]; then
    echo "Error! Exiting"
    exit 1
fi

echo "Copying required files into new project"
cp -r src/Assets $TEMP_UNITY_PATH

echo "Copying iOS files into new project"
mkdir -p $TEMP_UNITY_PATH/Assets/Plugins/iOS/
cp -r iOS/* $TEMP_UNITY_PATH/Assets/Plugins/iOS/
cp -r ../bugsnag-ios/Bugsnag\ Plugin/* $TEMP_UNITY_PATH/Assets/Plugins/iOS/
mv $TEMP_UNITY_PATH/Assets/Plugins/iOS/Categories/* $TEMP_UNITY_PATH/Assets/Plugins/iOS/
mv $TEMP_UNITY_PATH/Assets/Plugins/iOS/Dependencies/* $TEMP_UNITY_PATH/Assets/Plugins/iOS/
rm -rf $TEMP_UNITY_PATH/Assets/Plugins/iOS/Categories
rm -rf $TEMP_UNITY_PATH/Assets/Plugins/iOS/Dependencies

echo "Making categories Unity safe"
for i in $TEMP_UNITY_PATH/Assets/Plugins/iOS/*
do
  new_filename=$(echo $i | sed -e 's/\+/\-/')
  mv $i $new_filename
  
  old_filename=$(basename $i)
  # Escape the periods
  old_filename=$(echo $old_filename | sed -e 's/\./\\\./g')
  
  new_filename=$(basename $new_filename)
  # Escape the periods
  new_filename=$(echo $new_filename | sed -e 's/\./\\\./g')
  
  sed -i "" -e s/"$old_filename"/"$new_filename"/ $TEMP_UNITY_PATH/Assets/Plugins/iOS/*
done

echo "Copying android files into new project"
cd ../bugsnag-android
ant ndk
if [ $? -ne 0 ]; then
    echo "Error! Exiting"
    exit 1
fi

ant unity
if [ $? -ne 0 ]; then
    echo "Error! Exiting"
    exit 1
fi

cd $EXEC_PATH
mkdir -p $TEMP_UNITY_PATH/Assets/Plugins/Android
cp ../bugsnag-android/bugsnag-*-unity.jar $TEMP_UNITY_PATH/Assets/Plugins/Android/bugsnag-unity.jar
cp ../bugsnag-android/build/libs/armeabi/libbugsnag_bridge.so $TEMP_UNITY_PATH/Assets/Plugins/Android/libbugsnag.so

echo "Exporting unitypackage from temporary unity3d project"
${UNITY} -batchmode -quit -projectpath "$TEMP_UNITY_PATH" -exportpackage Assets "$BUILD_PATH/Bugsnag.unitypackage"
if [ $? -ne 0 ]; then
    echo "Error! Exiting"
    exit 1
fi

# Remove the temporary build directory
rm -rf $TEMP_UNITY_PATH

# Copy the built unitypackage to the root
cp "$BUILD_PATH/Bugsnag.unitypackage" .