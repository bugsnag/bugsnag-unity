package com.bugsnag.android.unity;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

import com.bugsnag.android.Error;
import com.bugsnag.android.Event;
import com.bugsnag.android.OnErrorCallback;

public class BugsnagUnity {
    public static OnErrorCallback getNativeCallback() {
        String discardedEventErrorClass = "java.lang.Error";
        Pattern pattern = Pattern.compile("signal \\d+ \\(SIG\\w+\\)", Pattern.CASE_INSENSITIVE);
        return new OnErrorCallback() {
            @Override
            // Discard any messages matching native Android events as they are captured (and more
            // accurate) via bugsnag-android. For Unity 2018.3+
            public boolean onError(Event event) {
                Error error = event.getErrors().get(0);
                String errorClass = error.getErrorClass();
                String message = error.getErrorMessage();
                if(discardedEventErrorClass.equals(errorClass)) {
                    return message == null || !pattern.matcher(message).find();
                }
                return true;
            }
        };
    }
}
