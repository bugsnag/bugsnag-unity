package com.bugsnag.android.unity;

import java.lang.String;
import java.util.Map;
import java.util.HashMap;

import android.content.Context;

import com.bugsnag.android.*;

public class UnityClient {

    public static void init(Context androidContext, String apiKey) {
        Bugsnag.init(androidContext, apiKey);
    }

    public static void notify(String name, String message,
                              String context, StackTraceElement[] stacktrace,
                              Severity severity, String logLevel,
                              String severityReason) {
        Throwable t = new BugsnagException(name, message, stacktrace);

        Map<String, Object> data = new HashMap<>();
        data.put("severity", severity.getName());
        data.put("severityReason", severityReason);

        Bugsnag.getClient().internalClientNotify(t, data, false, new UnityCallback(context, logLevel));
    }

}
