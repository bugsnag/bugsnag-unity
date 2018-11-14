package com.bugsnag.android.unity;

public class BugsnagUnity {
    static {
        System.loadLibrary("bugsnag-unity");
    }

    public static native boolean isJNIAttached();
}
