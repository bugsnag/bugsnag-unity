using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using UnityEngine;
using System.Threading;
using System.Text;

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
    private IntPtr StringClass;
    // Cache of methods used:
    private IntPtr BreadcrumbGetMessage;
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

    private bool CanRunOnBackgroundThread;

    private bool Unity2019OrNewer;
    private Thread MainThread;

    public NativeInterface(IConfiguration cfg)
    {
      AndroidJavaObject config = CreateNativeConfig(cfg);
      Unity2019OrNewer = IsUnity2019OrNewer();
      MainThread = Thread.CurrentThread;
      using (AndroidJavaClass system = new AndroidJavaClass("java.lang.System"))
      {
        string arch = system.CallStatic<string>("getProperty", "os.arch");
        CanRunOnBackgroundThread = (arch != "x86" && arch != "i686" && arch != "x86_64");
      }
      using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
      using (AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
      using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
      using (AndroidJavaObject client = new AndroidJavaObject("com.bugsnag.android.Client", context, config))
      {
        // lookup the NativeInterface class and set the client to the local object.
        // all subsequent communication should go through the NativeInterface.
        IntPtr nativeInterfaceRef = AndroidJNI.FindClass("com/bugsnag/android/NativeInterface");
        BugsnagNativeInterface = AndroidJNI.NewGlobalRef(nativeInterfaceRef);
        AndroidJNI.DeleteLocalRef(nativeInterfaceRef);

        IntPtr setClient = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, "setClient", "(Lcom/bugsnag/android/Client;)V");

        object[] args = new object[] {client};
        jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
        AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, setClient, jargs);
        AndroidJNIHelper.DeleteJNIArgArray(args, jargs);

        // Cache JNI refs which will be used to load report data later in the
        // app lifecycle to avoid repeated lookups
        IntPtr unityRef = AndroidJNI.FindClass("com/bugsnag/android/unity/BugsnagUnity");
        BugsnagUnityClass = AndroidJNI.NewGlobalRef(unityRef);
        AndroidJNI.DeleteLocalRef(unityRef);

        IntPtr crumbRef = AndroidJNI.FindClass("com/bugsnag/android/Breadcrumb");
        BreadcrumbClass = AndroidJNI.NewGlobalRef(crumbRef);
        AndroidJNI.DeleteLocalRef(crumbRef);

        IntPtr crumbTypeRef = AndroidJNI.FindClass("com/bugsnag/android/BreadcrumbType");
        BreadcrumbTypeClass = AndroidJNI.NewGlobalRef(crumbTypeRef);
        AndroidJNI.DeleteLocalRef(crumbTypeRef);

        IntPtr collectionRef = AndroidJNI.FindClass("java/util/Collection");
        CollectionClass = AndroidJNI.NewGlobalRef(collectionRef);
        AndroidJNI.DeleteLocalRef(collectionRef);

        IntPtr iterRef = AndroidJNI.FindClass("java/util/Iterator");
        IteratorClass = AndroidJNI.NewGlobalRef(iterRef);
        AndroidJNI.DeleteLocalRef(iterRef);

        IntPtr listRef = AndroidJNI.FindClass("java/util/List");
        ListClass = AndroidJNI.NewGlobalRef(listRef);
        AndroidJNI.DeleteLocalRef(listRef);

        IntPtr mapRef = AndroidJNI.FindClass("java/util/Map");
        MapClass = AndroidJNI.NewGlobalRef(mapRef);
        AndroidJNI.DeleteLocalRef(mapRef);

        IntPtr entryRef = AndroidJNI.FindClass("java/util/Map$Entry");
        MapEntryClass = AndroidJNI.NewGlobalRef(entryRef);
        AndroidJNI.DeleteLocalRef(entryRef);

        IntPtr setRef = AndroidJNI.FindClass("java/util/Set");
        SetClass = AndroidJNI.NewGlobalRef(setRef);
        AndroidJNI.DeleteLocalRef(setRef);

        IntPtr stringRef = AndroidJNI.FindClass("java/lang/String");
        StringClass = AndroidJNI.NewGlobalRef(stringRef);
        AndroidJNI.DeleteLocalRef(stringRef);

        BreadcrumbGetMetadata = AndroidJNI.GetMethodID(BreadcrumbClass, "getMetadata", "()Ljava/util/Map;");
        BreadcrumbGetType = AndroidJNI.GetMethodID(BreadcrumbClass, "getType", "()Lcom/bugsnag/android/BreadcrumbType;");
        BreadcrumbGetTimestamp = AndroidJNI.GetMethodID(BreadcrumbClass, "getStringTimestamp", "()Ljava/lang/String;");
        BreadcrumbGetMessage = AndroidJNI.GetMethodID(BreadcrumbClass, "getMessage", "()Ljava/lang/String;");
        CollectionIterator = AndroidJNI.GetMethodID(CollectionClass, "iterator", "()Ljava/util/Iterator;");
        IteratorHasNext = AndroidJNI.GetMethodID(IteratorClass, "hasNext", "()Z");
        IteratorNext = AndroidJNI.GetMethodID(IteratorClass, "next", "()Ljava/lang/Object;");
        MapEntryGetKey = AndroidJNI.GetMethodID(MapEntryClass, "getKey", "()Ljava/lang/Object;");
        MapEntryGetValue = AndroidJNI.GetMethodID(MapEntryClass, "getValue", "()Ljava/lang/Object;");
        MapEntrySet = AndroidJNI.GetMethodID(MapClass, "entrySet", "()Ljava/util/Set;");

        IntPtr objectRef = AndroidJNI.FindClass("java/lang/Object");
        ObjectToString = AndroidJNI.GetMethodID(objectRef, "toString", "()Ljava/lang/String;");
        ObjectGetClass = AndroidJNI.GetMethodID(objectRef, "getClass", "()Ljava/lang/Class;");
        AndroidJNI.DeleteLocalRef(objectRef);

        IntPtr classRef = AndroidJNI.FindClass("java/lang/Class");
        ClassIsArray = AndroidJNI.GetMethodID(classRef, "isArray", "()Z");
        AndroidJNI.DeleteLocalRef(classRef);

        // the bugsnag-android notifier uses Activity lifecycle tracking to
        // determine if the application is in the foreground. As the unity
        // activity has already started at this point we need to tell the
        // notifier about the activity and the fact that it has started.
        using (AndroidJavaObject sessionTracker = client.Get<AndroidJavaObject>("sessionTracker"))
        using (AndroidJavaObject activityClass = activity.Call<AndroidJavaObject>("getClass"))
        {
          string activityName = null;
          using(AndroidJavaObject activityNameObject = activityClass.Call<AndroidJavaObject>("getSimpleName"))
          {
            if (activityNameObject != null)
            {
              activityName = AndroidJNI.GetStringUTFChars(activityNameObject.GetRawObject());
            }
          }
          sessionTracker.Call("updateForegroundTracker", activityName, true, 0L);
        }

        ConfigureNotifierInfo(client);
      }
    }

    /**
     * Transforms an IConfiguration C# object into a Java Configuration object.
     */
    AndroidJavaObject CreateNativeConfig(IConfiguration config) {
      var obj = new AndroidJavaObject("com.bugsnag.android.Configuration", config.ApiKey);
      // configure automatic tracking of errors/sessions
      using (AndroidJavaObject errorTypes = new AndroidJavaObject("com.bugsnag.android.ErrorTypes"))
      {
        errorTypes.Call("setAnrs", config.AutoDetectAnrs);
        obj.Call("setEnabledErrorTypes", errorTypes);
      }
      obj.Call("setAutoTrackSessions", false);
      obj.Call("setAutoDetectErrors", config.AutoNotify);

      // set endpoints
      var notify = config.Endpoint.ToString();
      var sessions = config.SessionEndpoint.ToString();
      using (AndroidJavaObject endpointConfig = new AndroidJavaObject("com.bugsnag.android.EndpointConfiguration", notify, sessions))
      {
        obj.Call("setEndpoints", endpointConfig);
      }

      // set release stages
      obj.Call("setReleaseStage", config.ReleaseStage);

      if (config.NotifyReleaseStages != null) {
        using (AndroidJavaObject releaseStages = new AndroidJavaObject("java.util.HashSet"))
        {
          foreach(var element in config.NotifyReleaseStages)
          {
            using(AndroidJavaObject stage = BuildJavaStringDisposable(element))
            {
              releaseStages.Call<Boolean>("add", stage);
            }
          }
          obj.Call("setEnabledReleaseStages", releaseStages);
        }
      }

      // set version/context
      obj.Call("setAppVersion", config.AppVersion);
      obj.Call("setContext", config.Context);
      return obj;
    }

    private void ConfigureNotifierInfo(AndroidJavaObject client) {
      using (AndroidJavaObject notifier = client.Get<AndroidJavaObject>("notifier"))
      {
        notifier.Call("setUrl", NotifierInfo.NotifierUrl);
        notifier.Call("setName", "Bugsnag Unity (Android)");
        notifier.Call("setVersion", NotifierInfo.NotifierVersion);
      }
    }

    /**
     * Pushes a local JNI frame with 128 capacity. This avoids the reference table
     * being exceeded, which can happen on some lower-end Android devices in extreme conditions
     * (e.g. Nexus 7 running Android 6). This is likely due to AndroidJavaObject
     * not deleting local references immediately.
     *
     * If this call is unsuccessful it indicates the device is low on memory so the caller should no-op.
     * https://docs.unity3d.com/ScriptReference/AndroidJNI.PopLocalFrame.html
     */
    private bool PushLocalFrame() {
      if (AndroidJNI.PushLocalFrame(128) != 0) {
        AndroidJNI.ExceptionClear(); // clear pending OutOfMemoryError.
        return false;
      }
      return true;
    }

    /**
     * Pops the local JNI frame, freeing any references in the table.
     * https://docs.unity3d.com/ScriptReference/AndroidJNI.PopLocalFrame.html
     */
    private void PopLocalFrame() {
      AndroidJNI.PopLocalFrame(System.IntPtr.Zero);
    }

    public void SetAutoNotify(bool newValue) {
      CallNativeVoidMethod("setAutoNotify", "(Z)V", new object[]{newValue});
    }

    public void SetAutoDetectAnrs(bool newValue) {
      CallNativeVoidMethod("setAutoDetectAnrs", "(Z)V", new object[]{newValue});
    }

    public void SetContext(string newValue) {
      CallNativeVoidMethod("setContext", "(Ljava/lang/String;)V", new object[]{newValue});
    }

    public void SetUser(User user) {
      var method = "setUser";
      var description = "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V";
      if (user == null) {
        CallNativeVoidMethod(method, description, new object[]{null, null, null});
      } else {
        CallNativeVoidMethod(method, description,
            new object[]{user.Id, user.Email, user.Name});
      }
    }

    public void SetSession(Session session) {
      if (session == null) {
        // Clear session
        CallNativeVoidMethod("registerSession", "(JLjava/lang/String;II)V", new object[]{
          IntPtr.Zero, IntPtr.Zero, 0, 0
        });
      } else {
        // The ancient version of the runtime used doesn't have an equivalent to GetUnixTime()
        var startedAt = (session.StartedAt - new DateTime(1970, 1, 1, 0, 0, 0, 0)).Milliseconds;
        CallNativeVoidMethod("registerSession", "(JLjava/lang/String;II)V", new object[]{
          startedAt, session.Id.ToString(), session.UnhandledCount(),
          session.HandledCount()
        });
      }
    }

    public Dictionary<string, object> GetApp() {
      return GetJavaMapData("getApp");
    }

    public Dictionary<string, object> GetDevice() {
      return GetJavaMapData("getDevice");
    }

    public Dictionary<string, object> GetMetadata() {
      return GetJavaMapData("getMetadata");
    }

    public Dictionary<string, object> GetUser() {
      return GetJavaMapData("getUser");
    }

    public void RemoveMetadata(string tab) {
      if (tab == null) {
        return;
      }
      CallNativeVoidMethod("clearMetadata", "(Ljava/lang/String;Ljava/lang/String;)V", new object[]{tab, null});
    }

    public void AddToTab(string tab, string key, string value) {
      if (tab == null || key == null) {
        return;
      }
      CallNativeVoidMethod("addMetadata", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;)V",
          new object[]{tab, key, value});
    }

    public void LeaveBreadcrumb(string name, string type, IDictionary<string, string> metadata) {
      if (!CanRunJNI()) {
        return;
      }
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }
      if (PushLocalFrame()) {
        using (AndroidJavaObject map = BuildJavaMapDisposable(metadata))
        {
          CallNativeVoidMethod("leaveBreadcrumb", "(Ljava/lang/String;Ljava/lang/String;Ljava/util/Map;)V",
              new object[]{name, type, map});
        }
        PopLocalFrame();
      }
      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
    }

    public List<Breadcrumb> GetBreadcrumbs()
    {
      List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();
      if (!CanRunJNI()) {
        return breadcrumbs;
      }
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      IntPtr javaBreadcrumbs = CallNativeObjectMethodRef("getBreadcrumbs", "()Ljava/util/List;", new object[]{});
      IntPtr iterator = AndroidJNI.CallObjectMethod(javaBreadcrumbs, CollectionIterator, new jvalue[]{});
      AndroidJNI.DeleteLocalRef(javaBreadcrumbs);

      while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[]{}))
      {
        IntPtr crumb = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[]{});
        breadcrumbs.Add(ConvertToBreadcrumb(crumb));
        AndroidJNI.DeleteLocalRef(crumb);
      }
      AndroidJNI.DeleteLocalRef(iterator);

      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }

      return breadcrumbs;
    }

    private Dictionary<string, object> GetJavaMapData(string methodName) {
      if (!CanRunJNI()) {
        return new Dictionary<string, object>();
      }
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      IntPtr map = CallNativeObjectMethodRef(methodName, "()Ljava/util/Map;", new object[]{});
      Dictionary<string, object> value = DictionaryFromJavaMap(map);
      AndroidJNI.DeleteLocalRef(map);

      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }

      return value;
    }

    // Manually converts any C# strings in the arguments, replacing invalid chars with the replacement char..
    // If we don't do this, C# will coerce them using NewStringUTF, which crashes on invalid UTF-8.
    // Arg lists processed this way must be released using ReleaseConvertedStringArgs.
    private object[] ConvertStringArgsToNative(object[] args)
    {
      object[] itemsAsJavaObjects = new object[args.Length];
      for (int i = 0; i < args.Length; i++) {
        var obj = args[i];

        if (obj is string) {
          itemsAsJavaObjects[i] = BuildJavaStringDisposable(obj as string);
        } else {
          itemsAsJavaObjects[i] = obj;
        }
      }
      return itemsAsJavaObjects;
    }

    // Release any strings in a processed argument list.
    // @param originalArgs: The original C# args.
    // @param convertedArgs: The args list returned by ConvertStringArgsToNative.
    private void ReleaseConvertedStringArgs(object[] originalArgs, object[] convertedArgs)
    {
      for (int i = 0; i < originalArgs.Length; i++) {
        if (originalArgs[i] is string) {
          (convertedArgs[i] as AndroidJavaObject).Dispose();
        }
      }
    }

    private void CallNativeVoidMethod(string methodName, string methodSig, object[] args)
    {
      if (!CanRunJNI()) {
        return;
      }
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      object[] convertedArgs = ConvertStringArgsToNative(args);
      jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
      IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
      AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, methodID, jargs);
      AndroidJNIHelper.DeleteJNIArgArray(convertedArgs, jargs);
      ReleaseConvertedStringArgs(args, convertedArgs);

      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
    }

    private IntPtr CallNativeObjectMethodRef(string methodName, string methodSig, object[] args)
    {
      if (!CanRunJNI()) {
        return IntPtr.Zero;
      }
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      object[] convertedArgs = ConvertStringArgsToNative(args);
      jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
      IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
      IntPtr nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
      AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
      ReleaseConvertedStringArgs(args, convertedArgs);

      if (!isAttached) {
        AndroidJNI.DetachCurrentThread();
      }
      return nativeValue;
    }

    private IntPtr ConvertToStringJNIArrayRef(string[] items)
    {
      if (items == null || items.Length == 0) {
        return IntPtr.Zero;
      }

      AndroidJavaObject[] itemsAsJavaObjects = new AndroidJavaObject[items.Length];
      for (int i = 0; i < items.Length; i++) {
        itemsAsJavaObjects[i] = BuildJavaStringDisposable(items[i]);
      }

      AndroidJavaObject first = itemsAsJavaObjects[0];
      IntPtr rawArray = AndroidJNI.NewObjectArray(items.Length, StringClass, first.GetRawObject());
      first.Dispose();

      for (int i = 1; i < items.Length; i++) {
        AndroidJNI.SetObjectArrayElement(rawArray, i, itemsAsJavaObjects[i].GetRawObject());
        itemsAsJavaObjects[i].Dispose();
      }

      return rawArray;
    }

    private string CallNativeStringMethod(string methodName, string methodSig, object[] args)
    {
      if (!CanRunJNI()) {
        return "";
      }
      bool isAttached = bsg_unity_isJNIAttached();
      if (!isAttached) {
        AndroidJNI.AttachCurrentThread();
      }

      object[] convertedArgs = ConvertStringArgsToNative(args);
      jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
      IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
      IntPtr nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
      AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
      ReleaseConvertedStringArgs(args, convertedArgs);

      string value = null;
      if (nativeValue != null && nativeValue != IntPtr.Zero) {
        value = AndroidJNI.GetStringUTFChars(nativeValue);
      }
      AndroidJNI.DeleteLocalRef(nativeValue);

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

      IntPtr javaMetadata = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMetadata, new jvalue[]{});
      IntPtr entries = AndroidJNI.CallObjectMethod(javaMetadata, MapEntrySet, new jvalue[]{});
      AndroidJNI.DeleteLocalRef(javaMetadata);

      IntPtr iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[]{});
      AndroidJNI.DeleteLocalRef(entries);

      while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[]{}))
      {
        IntPtr entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[]{});
        IntPtr key = AndroidJNI.CallObjectMethod(entry, MapEntryGetKey, new jvalue[]{});
        IntPtr value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[]{});
        AndroidJNI.DeleteLocalRef(entry);

        if (key != IntPtr.Zero && value != IntPtr.Zero)
        {
          metadata.Add(AndroidJNI.GetStringUTFChars(key), AndroidJNI.GetStringUTFChars(value));
        }
        AndroidJNI.DeleteLocalRef(key);
        AndroidJNI.DeleteLocalRef(value);
      }
      AndroidJNI.DeleteLocalRef(iterator);

      IntPtr type = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetType, new jvalue[]{});
      string typeName = AndroidJNI.CallStringMethod(type, ObjectToString, new jvalue[]{});
      AndroidJNI.DeleteLocalRef(type);

      string message = "<empty>";
      IntPtr messageObj = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMessage, new jvalue[]{});
      if (messageObj != IntPtr.Zero)
      {
        message = AndroidJNI.GetStringUTFChars(messageObj);
      }
      AndroidJNI.DeleteLocalRef(messageObj);

      string timestamp = AndroidJNI.CallStringMethod(javaBreadcrumb, BreadcrumbGetTimestamp, new jvalue[]{});
      return new Breadcrumb(message, timestamp, typeName, metadata);
    }

    private AndroidJavaObject BuildJavaMapDisposable(IDictionary<string, string> src) {
      AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
      if (src != null)
      {
        foreach(var entry in src)
        {
          using(AndroidJavaObject key = BuildJavaStringDisposable(entry.Key))
          using(AndroidJavaObject value = BuildJavaStringDisposable(entry.Value))
          {
            map.Call<AndroidJavaObject>("put", key, value);
          }
        }
      }
      return map;
    }

    private AndroidJavaObject BuildJavaStringDisposable(string input) {
      if (input == null) {
        return null;
      }

      try {
        // The default encoding on Android is UTF-8
        using(AndroidJavaClass CharsetClass = new AndroidJavaClass("java.nio.charset.Charset"))
        using(AndroidJavaObject Charset = CharsetClass.CallStatic<AndroidJavaObject>("defaultCharset"))
        {
          byte[] Bytes = Encoding.UTF8.GetBytes(input);

          if (Unity2019OrNewer) { // should succeed on Unity 2019.1 and above
            sbyte[] SBytes = new sbyte[Bytes.Length];
            Buffer.BlockCopy(Bytes, 0, SBytes, 0, Bytes.Length);
            return new AndroidJavaObject("java.lang.String", SBytes, Charset);
          } else { // use legacy API on older versions
            return new AndroidJavaObject("java.lang.String", Bytes, Charset);
          }
        }
      } catch (EncoderFallbackException _) {
        // The input string could not be encoded as UTF-8
        return new AndroidJavaObject("java.lang.String");
      }
    }

    private bool IsUnity2019OrNewer() {
      using(AndroidJavaClass CharsetClass = new AndroidJavaClass("java.nio.charset.Charset"))
      using(AndroidJavaObject Charset = CharsetClass.CallStatic<AndroidJavaObject>("defaultCharset"))
      {
        try { // should succeed on Unity 2019.1 and above
          using(AndroidJavaObject obj = new AndroidJavaObject("java.lang.String", new sbyte[0], Charset))
          {
            return true;
          }
        } catch (System.Exception _) { // use legacy API on older versions
          return false;
        }
      }
    }

    private bool CanRunJNI() {
      return CanRunOnBackgroundThread || object.ReferenceEquals(Thread.CurrentThread, MainThread);
    }

    private Dictionary<string, object> DictionaryFromJavaMap(IntPtr source) {
      var dict = new Dictionary<string, object>();

      IntPtr entries = AndroidJNI.CallObjectMethod(source, MapEntrySet, new jvalue[]{});
      IntPtr iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[]{});
      AndroidJNI.DeleteLocalRef(entries);

      while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[]{}))
      {
        IntPtr entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[]{});
        string key = AndroidJNI.CallStringMethod(entry, MapEntryGetKey, new jvalue[]{});
        IntPtr value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[]{});
        AndroidJNI.DeleteLocalRef(entry);

        if (value != null && value != IntPtr.Zero)
        {
          IntPtr valueClass = AndroidJNI.CallObjectMethod(value, ObjectGetClass, new jvalue[]{});
          if (AndroidJNI.CallBooleanMethod(valueClass, ClassIsArray, new jvalue[]{}))
          {
            string[] values = AndroidJNIHelper.ConvertFromJNIArray<string[]>(value);
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
          AndroidJNI.DeleteLocalRef(valueClass);
        }
        AndroidJNI.DeleteLocalRef(value);
      }
      AndroidJNI.DeleteLocalRef(iterator);

      return dict;
    }
  }
}
