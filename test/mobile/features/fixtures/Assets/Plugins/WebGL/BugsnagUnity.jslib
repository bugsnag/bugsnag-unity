var BugsnagPlugin = {
    BSGRegister: function(apiKey, trackSessions)
    {
        Bugsnag.apiKey = Pointer_stringify(apiKey);
        // session tracking not enabled on webgl
    },
    BSGNotify: function(errorClass, errorMessage, severity, context, stackTrace, logType, severityReason)
    {
        var strErrorClass = Pointer_stringify(errorClass);
        var strErrorMessage = Pointer_stringify(errorMessage);
        var strStackTrace = Pointer_stringify(stackTrace);
        var strContext = Pointer_stringify(context);
        var strSeverity = Pointer_stringify(severity);
        var strLogType = Pointer_stringify(logType);
        var strSeverityReason = Pointer_stringify(severityReason);

        // Create a representative error with the provided stack trace
        var exp = new Error();
        exp.name = strErrorClass;
        exp.message = strErrorMessage;

        if (strStackTrace.length > 0) {
            var newStack = strErrorClass + ":" + strErrorMessage + "\n";
            strStackTrace.split("\n").forEach(function(element, index, array){
              if (element != "") {
                // Convert each stack trace line to a comparable javascript stack trace line
                var line = "    at " + element.split(" ")[0] + "(0:0)\n";
                newStack += line;
              }
            });
            newStack = newStack.slice(0,-1);
            exp.stack =  newStack;
        }

        metaData = {
          // Indicate that it's a unity exception
          "Unity" : {
            "unityException" : true
          },
          // Grab the app timing/inForeground data provided by BugsnagAppTimings.jspre
          "app": typeof __bugsnag__app_timings__ !== 'undefined' ? __bugsnag__app_timings__.summary() : {}
        };

        // Add the unity log level (if it came from a log)
        if (strLogType != null && strLogType != "") {
          metaData["Unity"]["unityLogLevel"] = strLogType;
        }

        var handledState = {
          originalSeverity: strSeverity,
          severityReason: {
            type: strSeverityReason
          },
          unhandled: false
        }

        Bugsnag.notifyException(exp, strErrorClass, metaData, strSeverity, handledState);
    },
    BSGSetNotifyUrl: function(notifyUrl)
    {
        Bugsnag.endpoint = Pointer_stringify(notifyUrl);
    },
    BSGSetAutoNotify: function(autoNotify)
    {
        Bugsnag.autoNotify = (autoNotify == 1 ? true : false);
    },
    BSGSetContext: function(context)
    {
        Bugsnag.context = Pointer_stringify(context);
    },
    BSGSetReleaseStage: function(releaseStage)
    {
        Bugsnag.releaseStage = Pointer_stringify(releaseStage);
    },
    BSGSetNotifyReleaseStages: function(releaseStages)
    {
        var stagesString = Pointer_stringify(releaseStages);
        Bugsnag.notifyReleaseStages = stagesString.split(',');
    },
    BSGAddToTab: function(tabName, attributeName, attributeValue)
    {
        var strTabName = Pointer_stringify(tabName);
        var strAttrName = Pointer_stringify(attributeName);
        var strAttrValue = Pointer_stringify(attributeValue);

        if (Bugsnag.metaData == null) {
            Bugsnag.metaData = {};
        }

        if (Bugsnag.metaData[strTabName] == null) {
            Bugsnag.metaData[strTabName] = {};
        }

        Bugsnag.metaData[strTabName][strAttrName] = strAttrValue;
    },
    BSGClearTab: function(tabName)
    {
        delete Bugsnag.metaData[Pointer_stringify(tabName)];
    },
    BSGLeaveBreadcrumb: function(breadcrumb)
    {
        // Do nothing as we don't support breadcrumbs for the js notifier
    },
    BSGSetBreadcrumbCapacity: function(capacity)
    {
        // Do nothing as we don't support breadcrumbs for the js notifier
    },
    BSGSetAppVersion: function(version)
    {
        Bugsnag.appVersion = Pointer_stringify(version)
    },
    BSGSetUser: function(userId, userName, userEmail)
    {
        var strUserId = Pointer_stringify(userId);
        var strUserName = Pointer_stringify(userName);
        var strUserEmail = Pointer_stringify(userEmail);

        Bugsnag.user = {
          id: strUserId,
          name: strUserName,
          email: strUserEmail
        };
    },
    BSGSetSessionUrl: function(sessionUrl)
    {
        // session tracking not enabled on webgl
    },
    BSGStartSession: function()
    {
        // session tracking not enabled on webgl
    }
};

mergeInto(LibraryManager.library, BugsnagPlugin);
