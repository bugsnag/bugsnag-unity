package com.bugsnag.android.unity;

import java.lang.String;

import android.content.Context;

import com.bugsnag.android.Bugsnag;
import com.bugsnag.android.Callback;
import com.bugsnag.android.Client;
import com.bugsnag.android.Error;
import com.bugsnag.android.MetaData;
import com.bugsnag.android.Report;
import com.bugsnag.android.Severity;

public class UnityClient {

    public static void init(Context androidContext, String apiKey) {
        Bugsnag.init(androidContext, apiKey);
    }

    public static void notify(String name, String message, String context, StackTraceElement[] stacktrace, Severity severity, String logLevel) {
        Bugsnag.notify(name, message, stacktrace, new UnityCallback(context, severity, logLevel));
    }
}

class UnityCallback implements Callback {
    static final String NOTIFIER_NAME = "Bugsnag Unity (Android)";
    static final String NOTIFIER_VERSION = "3.4.0";
    static final String NOTIFIER_URL = "https://github.com/bugsnag/bugsnag-unity";

    final private Severity severity;
    final private String context;
    final private String logLevel;

    UnityCallback(String context, Severity severity, String logLevel) {
        this.context = context;
        this.severity = severity;
        this.logLevel = logLevel;
    }

    @Override
    public void beforeNotify(Report report) {
        report.setNotifierName(NOTIFIER_NAME);
        report.setNotifierURL(NOTIFIER_URL);
        report.setNotifierVersion(NOTIFIER_VERSION);

        Error error = report.getError();
        error.setSeverity(severity);
        MetaData metadata = error.getMetaData();
        metadata.addToTab("Unity", "unityException", "true");
        if (logLevel != null && logLevel.length() > 0)
            metadata.addToTab("Unity", "unityLogLevel", logLevel);
        if (context != null && context.length() > 0)
            error.setContext(context);
    }
}
