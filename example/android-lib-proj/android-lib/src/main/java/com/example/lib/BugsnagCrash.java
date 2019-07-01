package com.example.lib;

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

    public native void NdkCrash();
}
