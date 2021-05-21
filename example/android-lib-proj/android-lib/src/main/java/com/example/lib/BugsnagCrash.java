package com.example.lib;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;

public class BugsnagCrash {

    static {
        System.loadLibrary("unity-example-lib");
    }

    public void throwJvmException() {
        throw new RuntimeException("Uncaught JVM exception");
    }

    public void throwBackgroundJvmException() {
        new Thread(new Runnable() {
            public void run() {
                throw new RuntimeException("Uncaught JVM exception from background thread");
            }
        }).start();
    }

    public void triggerAnr() {
        Log.d("Bugsnag", "Performing ANR in Unity app");
        Handler handler = new Handler(Looper.getMainLooper());
        handler.post(new Runnable() {
            @Override
            public void run() {
                try {
                    Thread.sleep(10000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    public native void raiseNdkSignal();

    public native void throwCppException();
}
