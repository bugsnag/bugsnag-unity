package com.example.bugsnagcrashplugin;
import android.util.Log;
import android.os.Looper;

public class CrashHelper {

    static {
        System.loadLibrary("entrypoint");
    }

    public static void triggerJvmException() {
        String[] items = {"one", "two"};
        Log.i("NativePlugin", "This is the third item: " + items[2]);
    }

     public static void triggerBackgroundJvmException() {
        new Thread(new Runnable() {
            public void run() {
                throw new RuntimeException("Uncaught JVM exception from background thread");
            }
        }).start();
    }

    public static native void raiseNdkSignal();
}
