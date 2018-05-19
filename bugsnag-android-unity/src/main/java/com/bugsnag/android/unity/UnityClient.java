package com.bugsnag.android.unity;

import java.lang.String;
import java.util.Map;
import java.util.HashMap;

import android.content.Context;

import com.bugsnag.android.*;

public class UnityClient {

    static volatile Client client;
    private static final Object lock = new Object();

    public static void init(Context androidContext, String apiKey, boolean trackSessions) {
        if (client == null) {
            synchronized (lock) {
                if (client == null) {
                    Configuration config = new Configuration(apiKey);
                    config.setAutoCaptureSessions(trackSessions);
                    client = Bugsnag.init(androidContext, config);
                }
            }
        }
    }

    public static void notify(String name, String message,
                              String context, StackTraceElement[] stacktrace,
                              Severity severity, String logLevel,
                              String severityReason) {
        if (client == null) {
            return;
        }
        Throwable t = new BugsnagException(name, message, stacktrace);

        Map<String, Object> data = new HashMap<>();
        data.put("severity", severity.getName());
        data.put("severityReason", severityReason);
        data.put("logLevel", logLevel);

        client.internalClientNotify(t, data, false, new UnityCallback(context, logLevel));
    }

    public static void setContext(String context) {
        if (client != null) {
            client.setContext(context);
        }
    }

    public static void setAutoNotify(boolean autoNotify) {
        if (client != null) {
            if (autoNotify) {
                client.enableExceptionHandler();
            } else {
                client.disableExceptionHandler();
            }
        }
    }

    public static void setEndpoint(String endpoint) {
        if (client != null) {
            Configuration config = client.getConfig();
            config.setEndpoint(endpoint);
        }
    }

    public static void setSessionEndpoint(String endpoint) {
        if (client != null) {
            Configuration config = client.getConfig();
            config.setSessionEndpoint(endpoint);
        }
    }

    public static void setUser(String userId, String userName, String userEmail) {
        if (client != null) {
            client.setUser(userId, userName, userEmail);
        }
    }

    public static void startSession() {
        if (client != null) {
            client.startSession();
        }
    }

    public static void setAppVersion(String version) {
        if (client != null) {
            client.setAppVersion(version);
        }
    }

    public static void setMaxBreadcrumbs(int numBreadcrumbs) {
        if (client != null) {
            client.setMaxBreadcrumbs(numBreadcrumbs);
        }
    }

    public static void leaveBreadcrumb(String breadcrumb) {
        if (client != null) {
            client.leaveBreadcrumb(breadcrumb);
        }
    }

    public static void setReleaseStage(String releaseStage) {
        if (client != null) {
            client.setReleaseStage(releaseStage);
        }
    }

    public static void setNotifyReleaseStages(String... notifyReleaseStages) {
        if (client != null) {
            client.setNotifyReleaseStages(notifyReleaseStages);
        }
    }

    public static void addToTab(String tab, String key, Object value) {
        if (client != null) {
            client.addToTab(tab, key, value);
        }
    }

    public static void clearTab(String tabName) {
        if (client != null) {
            client.clearTab(tabName);
        }
    }
}
