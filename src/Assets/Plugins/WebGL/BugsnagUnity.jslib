var BugsnagPlugin = {
    Register: function(apiKey)
    {
        Bugsnag.apiKey = Pointer_stringify(apiKey);
    },
    Notify: function(errorClass, errorMessage, severity, context, stackTrace, logType, severityReason)
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

        // Indicate that its a unity exception, along with the received log level
        metaData = {
          "Unity" : {
            "unityException" : true
          }
        };

        if (strLogType != null && strLogType != "") {
          metaData["Unity"]["unityLogLevel"] = strLogType;
        }

        // TODO pass in severityReason here as well!
        Bugsnag.notifyException(exp, strErrorClass, metaData, strSeverity);
    },
    SetNotifyUrl: function(notifyUrl)
    {
        Bugsnag.endpoint = Pointer_stringify(notifyUrl);
    },
    SetAutoNotify: function(autoNotify)
    {
        Bugsnag.autoNotify = (autoNotify == 1 ? true : false);
    },
    SetContext: function(context)
    {
        Bugsnag.context = Pointer_stringify(context);
    },
    SetReleaseStage: function(releaseStage)
    {
        Bugsnag.releaseStage = Pointer_stringify(releaseStage);
    },
    SetNotifyReleaseStages: function(releaseStages)
    {
        var stagesString = Pointer_stringify(releaseStages);
        Bugsnag.notifyReleaseStages = stagesString.split(',');
    },
    AddToTab: function(tabName, attributeName, attributeValue)
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
    ClearTab: function(tabName)
    {
        delete Bugsnag.metaData[Pointer_stringify(tabName)];
    },
    LeaveBreadcrumb: function(breadcrumb)
    {
        // Do nothing as we don't support breadcrumbs for the js notifier
    },
    SetBreadcrumbCapacity: function(capacity)
    {
        // Do nothing as we don't support breadcrumbs for the js notifier
    },
    SetAppVersion: function(version)
    {
        Bugsnag.appVersion = Pointer_stringify(version)
    },
    SetUser: function(userId, userName, userEmail)
    {
        var strUserId = Pointer_stringify(userId);
        var strUserName = Pointer_stringify(userName);
        var strUserEmail = Pointer_stringify(userEmail);

        Bugsnag.user = {
          id: strUserId,
          name: strUserName,
          email: strUserEmail
        };
    }
};

mergeInto(LibraryManager.library, BugsnagPlugin);
