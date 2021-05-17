package com.example.bugsnagcrashplugin;
import android.util.Log;

public class CrashHelper {

    static {
        System.loadLibrary("entrypoint");
    }

    public static void triggerJvmException() {
        String[] items = {"one", "two"};
        Log.i("NativePlugin", "This is the third item: " + items[2]);
    }

    public static native void raiseNdkSignal();
}
