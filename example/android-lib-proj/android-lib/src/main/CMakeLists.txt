add_library( # Specifies the name of the library.
             unity-example-lib

             # Sets the library as a shared library.
             SHARED

             # Provides a relative path to your source file(s).
             jni/entrypoint.cpp
             )

include_directories(jni/)
