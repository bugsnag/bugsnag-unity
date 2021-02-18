package com.example.lib;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;

public class BugsnagCrash {

    static {
        System.loadLibrary("unity-example-lib");
    }

    public void Crash() {
        throw new RuntimeException("from java");
    }

    public void BackgroundCrash() {
        new Thread(new Runnable() {
            public void run() {
                throw new RuntimeException("from java background thread");
            }
        }).start();
    }

    public void AnrCrash() {
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

    public native void NdkSignal();
    public native void NdkException();
}
