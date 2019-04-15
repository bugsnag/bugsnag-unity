This is an Android Library project which can be used to assemble an AAR.

Running `./gradlew assemble` will package an AAR containing all code from this module, and place it
in `Assets/Plugins/Android`, so that Unity can automatically access any JVM classes via the JNI.
