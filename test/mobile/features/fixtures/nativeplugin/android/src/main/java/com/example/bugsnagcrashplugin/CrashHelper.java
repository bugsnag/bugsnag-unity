package com.example.bugsnagcrashplugin;
import android.util.Log;

public class CrashHelper {
    public static void UnhandledCrash ()
    {
        String[] items = {"one", "two"};
        Log.i("NativePlugin", "This is the third item: " + items[2]);
    }
}
