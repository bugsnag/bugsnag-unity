package com.bugsnag.android.unity;

import java.lang.String;

import android.content.Context;

import com.bugsnag.android.*;

public class UnityClient {

    public static void init(Context androidContext, String apiKey) {
        Bugsnag.init(androidContext, apiKey);
    }

    public static void notify(String name, String message, String context, StackTraceElement[] stacktrace, Severity severity, String logLevel) {
        Bugsnag.notify(name, message, stacktrace, new UnityCallback(context, severity, logLevel));
    }
}

