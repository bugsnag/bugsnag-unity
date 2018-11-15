using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
  class NativeInterface
  {
    private IntPtr BugsnagNativeInterface;
    private IntPtr BugsnagUnityClass;
    // Cache of classes used:
    private IntPtr BreadcrumbClass;
    private IntPtr BreadcrumbTypeClass;
    private IntPtr CollectionClass;
    private IntPtr IteratorClass;
    private IntPtr ListClass;
    private IntPtr MapClass;
    private IntPtr MapEntryClass;
    private IntPtr SetClass;
    // Cache of methods used:
    private IntPtr BreadcrumbGetName;
    private IntPtr BreadcrumbGetMetadata;
    private IntPtr BreadcrumbGetTimestamp;
    private IntPtr BreadcrumbGetType;
    private IntPtr ClassIsArray;
    private IntPtr CollectionIterator;
    private IntPtr IteratorHasNext;
    private IntPtr IteratorNext;
    private IntPtr MapEntryGetKey;
    private IntPtr MapEntryGetValue;
    private IntPtr MapEntrySet;
    private IntPtr ObjectGetClass;
    private IntPtr ObjectToString;

    public NativeInterface(AndroidJavaObject config)
    {
      using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
      using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
      {
        // lookup the NativeInterface class and set the client to the local object.
        // all subsequent communication should go through the NativeInterface.
        var client = new AndroidJavaObject("com.bugsnag.android.Client", context, config);
        var nativeInterfaceRef = AndroidJNI.FindClass("com/bugsnag/android/NativeInterface");
        BugsnagNativeInterface = AndroidJNI.NewGlobalRef(nativeInterfaceRef);
        AndroidJNI.DeleteLocalRef(nativeInterfaceRef);
        var setClient = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, "setClient", "(Lcom/bugsnag/android/Client;)V");

        jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[] {client});
        AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, setClient, args);

        // Cache JNI refs which will be used to load report data later in the
        // app lifecycle to avoid repeated lookups
        var unityRef = AndroidJNI.FindClass("com/bugsnag/android/unity/BugsnagUnity");
        BugsnagUnityClass = AndroidJNI.NewGlobalRef(unityRef);
        AndroidJNI.DeleteLocalRef(unityRef);
        var crumbRef = AndroidJNI.FindClass("com/bugsnag/android/Breadcrumb");
        BreadcrumbClass = AndroidJNI.NewGlobalRef(crumbRef);
        AndroidJNI.DeleteLocalRef(crumbRef);
        var crumbTypeRef = AndroidJNI.FindClass("com/bugsnag/android/BreadcrumbType");
        BreadcrumbTypeClass = AndroidJNI.NewGlobalRef(crumbTypeRef);
        AndroidJNI.DeleteLocalRef(crumbTypeRef);
        var collectionRef = AndroidJNI.FindClass("java/util/Collection");
        CollectionClass = AndroidJNI.NewGlobalRef(collectionRef);
        AndroidJNI.DeleteLocalRef(collectionRef);
        var iterRef = AndroidJNI.FindClass("java/util/Iterator");
        IteratorClass = AndroidJNI.NewGlobalRef(iterRef);
        AndroidJNI.DeleteLocalRef(iterRef);
        var listRef = AndroidJNI.FindClass("java/util/List");
        ListClass = AndroidJNI.NewGlobalRef(listRef);
        AndroidJNI.DeleteLocalRef(listRef);
        var mapRef = AndroidJNI.FindClass("java/util/Map");
        MapClass = AndroidJNI.NewGlobalRef(mapRef);
        AndroidJNI.DeleteLocalRef(mapRef);
        var entryRef = AndroidJNI.FindClass("java/util/Map$Entry");
        MapEntryClass = AndroidJNI.NewGlobalRef(entryRef);
        AndroidJNI.DeleteLocalRef(entryRef);
        var setRef = AndroidJNI.FindClass("java/util/Set");
        SetClass = AndroidJNI.NewGlobalRef(setRef);
        AndroidJNI.DeleteLocalRef(setRef);

        BreadcrumbGetMetadata = AndroidJNI.GetMethodID(BreadcrumbClass, "getMetadata", "()Ljava/util/Map;");
        BreadcrumbGetType = AndroidJNI.GetMethodID(BreadcrumbClass, "getType", "()Lcom/bugsnag/android/BreadcrumbType;");
        BreadcrumbGetTimestamp = AndroidJNI.GetMethodID(BreadcrumbClass, "getTimestamp", "()Ljava/lang/String;");
        BreadcrumbGetName = AndroidJNI.GetMethodID(BreadcrumbClass, "getName", "()Ljava/lang/String;");
        CollectionIterator = AndroidJNI.GetMethodID(CollectionClass, "iterator", "()Ljava/util/Iterator;");
        IteratorHasNext = AndroidJNI.GetMethodID(IteratorClass, "hasNext", "()Z");
        IteratorNext = AndroidJNI.GetMethodID(IteratorClass, "next", "()Ljava/lang/Object;");
        MapEntryGetKey = AndroidJNI.GetMethodID(MapEntryClass, "getKey", "()Ljava/lang/Object;");
        MapEntryGetValue = AndroidJNI.GetMethodID(MapEntryClass, "getValue", "()Ljava/lang/Object;");
        MapEntrySet = AndroidJNI.GetMethodID(MapClass, "entrySet", "()Ljava/util/Set;");
        var objectRef = AndroidJNI.FindClass("java/lang/Object");
        ObjectToString = AndroidJNI.GetMethodID(objectRef, "toString", "()Ljava/lang/String;");
        ObjectGetClass = AndroidJNI.GetMethodID(objectRef, "getClass", "()Ljava/lang/Class;");
        AndroidJNI.DeleteLocalRef(objectRef);
        var classRef = AndroidJNI.FindClass("java/lang/Class");
        ClassIsArray = AndroidJNI.GetMethodID(classRef, "isArray", "()Z");
        AndroidJNI.DeleteLocalRef(classRef);

        // the bugsnag-android notifier uses Activity lifecycle tracking to
        // determine if the application is in the foreground. As the unity
        // activity has already started at this point we need to tell the
        // notifier about the activity and the fact that it has started.
        using (var sessionTracker = client.Get<AndroidJavaObject>("sessionTracker"))
        using (var activityClass = activity.Call<AndroidJavaObject>("getClass"))
        {
          string activityName = null;
          var activityNameObject = activityClass.Call<AndroidJavaObject>("getSimpleName");
          if (activityNameObject != null)
          {
            activityName = AndroidJNI.GetStringUTFChars(activityNameObject.GetRawObject());
          }
          sessionTracker.Call("updateForegroundTracker", activityName, true, 0L);
        }
      }
    }

    public string GetAppVersion() {
      return CallNativeStringMethod("getAppVersion", "()Ljava/lang/String;", new object[]{});
    }

    public string GetEndpoint() {
      return CallNativeStringMethod("getEndpoint", "()Ljava/lang/String;", new object[]{});
    }

    public string GetSessionEndpoint() {
      return CallNativeStringMethod("getSessionEndpoint", "()Ljava/lang/String;", new object[]{});
    }

    public string GetReleaseStage() {
      return CallNativeStringMethod("getReleaseStage", "()Ljava/lang/String;", new object[]{});
    }

    public string GetContext() {
      return CallNativeStringMethod("getContext", "()Ljava/lang/String;", new object[]{});
    }

    public void SetContext(string newValue) {
      CallNativeVoidMethod("setContext", "(Ljava/lang/String;)V", new object[]{newValue});
    }

    public void SetReleaseStage(string newValue) {
      CallNativeVoidMethod("setReleaseStage", "(Ljava/lang/String;)V", new object[]{newValue});
    }

    public void SetSessionEndpoint(string newValue) {
      CallNativeVoidMethod("setSessionEndpoint", "(Ljava/lang/String;)V", new object[]{newValue});
    }

    public void SetEndpoint(string newValue) {
      CallNativeVoidMethod("setEndpoint", "(Ljava/lang/String;)V", new object[]{newValue});
    }

    public void SetAppVersion(string newValue) {
      CallNativeVoidMethod("setAppVersion", "(Ljava/lang/String;)V", new object[]{newValue});
    }

    public void SetNotifyReleaseStages(string[] stages) {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }
      var newValue = AndroidJNIHelper.ConvertToJNIArray(stages);
      CallNativeVoidMethod("setNotifyReleaseStages", "([Ljava/lang/String;)V", new object[]{newValue});
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
    }

    public Dictionary<string, object> GetAppData() {
      return GetJavaMapData("getAppData");
    }

    public Dictionary<string, object> GetDeviceData() {
      return GetJavaMapData("getDeviceData");
    }

    public Dictionary<string, object> GetMetaData() {
      return GetJavaMapData("getMetaData");
    }

    public Dictionary<string, object> GetUser() {
      return GetJavaMapData("getUserData");
    }

    public void AddToTab(string tab, string key, string value) {
      CallNativeVoidMethod("addToTab", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;)V", new object[]{tab, key, value});
    }

    public void LeaveBreadcrumb(string name, string type, IDictionary<string, string> metadata) {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }
      using (var map = JavaMapFromDictionary(metadata))
      {
        CallNativeVoidMethod("leaveBreadcrumb", "(Ljava/lang/String;Ljava/lang/String;Ljava/util/Map;)V", new object[]{name, type, map});
      }
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
    }

    public string[] GetNotifyReleaseStages() {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }
      var stages = CallNativeObjectMethod("getNotifyReleaseStages", "()[Ljava/lang/String;", new object[]{});
      string[] value = null;
      if (stages != null && stages != IntPtr.Zero)
      {
        value = AndroidJNIHelper.ConvertFromJNIArray<string[]>(stages);
      }
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
      return value;
    }

    public List<Breadcrumb> GetBreadcrumbs()
    {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();

      var javaBreadcrumbs = CallNativeObjectMethod("getBreadcrumbs", "()Ljava/util/List;", new object[]{});
      var iterator = AndroidJNI.CallObjectMethod(javaBreadcrumbs, CollectionIterator, new jvalue[]{});

      while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[]{}))
      {
        var crumb = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[]{});
        breadcrumbs.Add(ConvertToBreadcrumb(crumb));
        AndroidJNI.DeleteLocalRef(crumb);
      }

      AndroidJNI.DeleteLocalRef(javaBreadcrumbs);
      AndroidJNI.DeleteLocalRef(iterator);
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }

      return breadcrumbs;
    }

    private void CallNativeVoidMethod(string methodName, string methodSig, object[] args)
    {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      var jargs = AndroidJNIHelper.CreateJNIArgArray(args);
      var methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
      AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, methodID, jargs);
      AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
    }

    private IntPtr CallNativeObjectMethod(string methodName, string methodSig, object[] args)
    {
      var jargs = AndroidJNIHelper.CreateJNIArgArray(args);
      var methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
      var nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
      AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
      return nativeValue;
    }

    private string CallNativeStringMethod(string methodName, string methodSig, object[] args)
    {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }
      var jargs = AndroidJNIHelper.CreateJNIArgArray(args);
      var methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
      var nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
      AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
      string value = null;
      if (nativeValue != null && nativeValue != IntPtr.Zero) {
        value = AndroidJNI.GetStringUTFChars(nativeValue);
      }
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
      return value;
    }

    private Dictionary<string, object> GetJavaMapData(string methodName) {
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }
      var value = DictionaryFromJavaMap(CallNativeObjectMethod(methodName, "()Ljava/util/Map;", new object[]{}));
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }

      return value;
    }

    [DllImport ("bugsnag-unity")]
    private static extern bool bsg_unity_isJNIAttached();

    private Breadcrumb ConvertToBreadcrumb(IntPtr javaBreadcrumb)
    {
      var metadata = new Dictionary<string, string>();

      var javaMetadata = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMetadata, new jvalue[]{});
      var entries = AndroidJNI.CallObjectMethod(javaMetadata, MapEntrySet, new jvalue[]{});
      var iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[]{});

      while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[]{}))
      {
        var entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[]{});
        var key = AndroidJNI.CallObjectMethod(entry, MapEntryGetKey, new jvalue[]{});
        var value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[]{});
        if (key != IntPtr.Zero && value != IntPtr.Zero)
        {
          metadata.Add(AndroidJNI.GetStringUTFChars(key), AndroidJNI.GetStringUTFChars(value));
        }
        AndroidJNI.DeleteLocalRef(key);
        AndroidJNI.DeleteLocalRef(value);
        AndroidJNI.DeleteLocalRef(entry);
      }

      AndroidJNI.DeleteLocalRef(javaMetadata);
      AndroidJNI.DeleteLocalRef(entries);
      AndroidJNI.DeleteLocalRef(iterator);

      var type = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetType, new jvalue[]{});
      var typeName = AndroidJNI.CallStringMethod(type, ObjectToString, new jvalue[]{});
      AndroidJNI.DeleteLocalRef(type);
      var name = "<empty>";
      var nameObj = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetName, new jvalue[]{});
      if (nameObj != IntPtr.Zero)
      {
        name = AndroidJNI.GetStringUTFChars(nameObj);
      }
      var timestamp = AndroidJNI.CallStringMethod(javaBreadcrumb, BreadcrumbGetTimestamp, new jvalue[]{});

      return new Breadcrumb(name, timestamp, typeName, metadata);
    }

    private AndroidJavaObject JavaMapFromDictionary(IDictionary<string, string> src) {
      var map = new AndroidJavaObject("java.util.HashMap");
      if (src != null)
      {
        foreach(var entry in src)
        {
          map.Call<AndroidJavaObject>("put", entry.Key, entry.Value);
        }
      }
      return map;
    }

    private Dictionary<string, object> DictionaryFromJavaMap(IntPtr source) {
      var dict = new Dictionary<string, object>();

      var entries = AndroidJNI.CallObjectMethod(source, MapEntrySet, new jvalue[]{});
      var iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[]{});

      while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[]{}))
      {
        var entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[]{});
        var key = AndroidJNI.CallStringMethod(entry, MapEntryGetKey, new jvalue[]{});
        var value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[]{});
        if (value != null && value != IntPtr.Zero)
        {
          var valueClass = AndroidJNI.CallObjectMethod(value, ObjectGetClass, new jvalue[]{});
          if (AndroidJNI.CallBooleanMethod(valueClass, ClassIsArray, new jvalue[]{}))
          {
            var values = AndroidJNIHelper.ConvertFromJNIArray<string[]>(value);
            dict.AddToPayload(key, values);
          }
          else if (AndroidJNI.IsInstanceOf(value, MapClass))
          {
            dict.AddToPayload(key, DictionaryFromJavaMap(value));
          }
          else
          {
            // FUTURE(dm): check if Integer, Long, Double, or Float before calling toString
            dict.AddToPayload(key, AndroidJNI.CallStringMethod(value, ObjectToString, new jvalue[]{}));
          }
          AndroidJNI.DeleteLocalRef(value);
        }
        AndroidJNI.DeleteLocalRef(entry);
      }
      return dict;
    }
  }
}
