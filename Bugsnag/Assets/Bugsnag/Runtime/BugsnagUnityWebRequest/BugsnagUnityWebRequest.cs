// VERSION 2.0.0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace BugsnagNetworking
{

    public class BugsnagUnityWebRequest : IDisposable
    {

        public static RequestEvent OnSend = new RequestEvent();

        public static RequestEvent OnComplete = new RequestEvent();

        public static RequestEvent OnAbort = new RequestEvent();

        public UnityWebRequest UnityWebRequest;

        // Constructors
        public BugsnagUnityWebRequest()
        {
            UnityWebRequest = new UnityWebRequest();
        }

        public BugsnagUnityWebRequest(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest = unityWebRequest;
        }

        public BugsnagUnityWebRequest(string url)
        {
            UnityWebRequest = new UnityWebRequest(url);
        }

        public BugsnagUnityWebRequest(Uri uri)
        {
            UnityWebRequest = new UnityWebRequest(uri);
        }

        public BugsnagUnityWebRequest(string url, string method)
        {
            UnityWebRequest = new UnityWebRequest(url, method);
        }

        public BugsnagUnityWebRequest(Uri uri, string method)
        {
            UnityWebRequest = new UnityWebRequest(uri, method);
        }

        // Static Constructors

        // Get
        public static BugsnagUnityWebRequest Get(string uri)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Get(uri));
        }

        public static BugsnagUnityWebRequest Get(Uri uri)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Get(uri));
        }

        // Post
#if UNITY_2022_2_OR_NEWER
        public static BugsnagUnityWebRequest PostWwwForm(string uri, string form)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.PostWwwForm(uri, form));
        }
#else
        public static BugsnagUnityWebRequest Post(string uri, string postData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, postData));
        }
#endif


        public static BugsnagUnityWebRequest Post(string uri, WWWForm formData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, formData));
        }

        public static BugsnagUnityWebRequest Post(string uri, List<IMultipartFormSection> multipartFormSections)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, multipartFormSections));
        }

        public static BugsnagUnityWebRequest Post(string uri, Dictionary<string, string> formFields)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, formFields));
        }

#if UNITY_2022_2_OR_NEWER
        public static BugsnagUnityWebRequest PostWwwForm(Uri uri, string form)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.PostWwwForm(uri, form));
        }
#else
        public static BugsnagUnityWebRequest Post(Uri uri, string postData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, postData));
        }
#endif


        public static BugsnagUnityWebRequest Post(Uri uri, WWWForm formData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, formData));
        }

        public static BugsnagUnityWebRequest Post(Uri uri, List<IMultipartFormSection> multipartFormSections)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, multipartFormSections));
        }

        public static BugsnagUnityWebRequest Post(Uri uri, Dictionary<string, string> formFields)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, formFields));
        }

        public static BugsnagUnityWebRequest Post(string uri, List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, multipartFormSections, boundary));
        }

        public static BugsnagUnityWebRequest Post(Uri uri, List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Post(uri, multipartFormSections, boundary));
        }

        // Put
        public static BugsnagUnityWebRequest Put(string uri, byte[] bodyData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        public static BugsnagUnityWebRequest Put(string uri, string bodyData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        public static BugsnagUnityWebRequest Put(Uri uri, byte[] bodyData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        public static BugsnagUnityWebRequest Put(Uri uri, string bodyData)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Put(uri, bodyData));
        }

        // Head
        public static BugsnagUnityWebRequest Head(string uri)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Head(uri));
        }

        public static BugsnagUnityWebRequest Head(Uri uri)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Head(uri));
        }

        // Delete
        public static BugsnagUnityWebRequest Delete(string uri)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Delete(uri));
        }

        public static BugsnagUnityWebRequest Delete(Uri uri)
        {
            return new BugsnagUnityWebRequest(UnityWebRequest.Delete(uri));
        }

        // Static Methods

        public static void ClearCookieCache()
        {
            UnityWebRequest.ClearCookieCache();
        }

        public static string EscapeURL(string s)
        {
            return UnityWebRequest.EscapeURL(s);
        }

        public static string EscapeURL(string s, Encoding e)
        {
            return UnityWebRequest.EscapeURL(s, e);
        }

        public static string UnEscapeURL(string s)
        {
            return UnityWebRequest.UnEscapeURL(s);
        }

        public static string UnEscapeURL(string s, Encoding e)
        {
            return UnityWebRequest.UnEscapeURL(s, e);
        }

        public static byte[] GenerateBoundary()
        {
            return UnityWebRequest.GenerateBoundary();
        }

        public static byte[] SerializeFormSections(List<IMultipartFormSection> multipartFormSections, byte[] boundary)
        {
            return UnityWebRequest.SerializeFormSections(multipartFormSections, boundary);
        }

        public static byte[] SerializeSimpleForm(Dictionary<string, string> formFields)
        {
            return UnityWebRequest.SerializeSimpleForm(formFields);
        }


        // Public methods

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            OnSend.Invoke(this);
            var asyncAction = UnityWebRequest.SendWebRequest();
            asyncAction.completed += RequestCompleted;
            return asyncAction;
        }

        private void RequestCompleted(AsyncOperation obj)
        {
            OnComplete.Invoke(this);
        }

        public void Abort()
        {
            OnAbort.Invoke(this);
            UnityWebRequest.Abort();
        }

        public string GetRequestHeader(string name) => UnityWebRequest.GetRequestHeader(name);

        public string GetResponseHeader(string name) => UnityWebRequest.GetResponseHeader(name);

        public Dictionary<string, string> GetResponseHeaders() => UnityWebRequest.GetResponseHeaders();

        public void SetRequestHeader(string name, string value) => UnityWebRequest.SetRequestHeader(name, value);

        public void Dispose() => UnityWebRequest.Dispose();

        // Static Properties

        public static string kHttpVerbCREATE => UnityWebRequest.kHttpVerbCREATE;

        public static string kHttpVerbDELETE => UnityWebRequest.kHttpVerbDELETE;

        public static string kHttpVerbGET => UnityWebRequest.kHttpVerbGET;

        public static string kHttpVerbHEAD => UnityWebRequest.kHttpVerbHEAD;

        public static string kHttpVerbPOST => UnityWebRequest.kHttpVerbPOST;

        public static string kHttpVerbPUT => UnityWebRequest.kHttpVerbPUT;

        // Properties

        public UnityEngine.Networking.CertificateHandler certificateHandler
        {
            get { return UnityWebRequest.certificateHandler; }
            set { UnityWebRequest.certificateHandler = value; }
        }

        public bool disposeCertificateHandlerOnDispose
        {
            get { return UnityWebRequest.disposeCertificateHandlerOnDispose; }
            set { UnityWebRequest.disposeCertificateHandlerOnDispose = value; }
        }

        public bool disposeDownloadHandlerOnDispose
        {
            get { return UnityWebRequest.disposeDownloadHandlerOnDispose; }
            set { UnityWebRequest.disposeDownloadHandlerOnDispose = value; }
        }

        public bool disposeUploadHandlerOnDispose
        {
            get { return UnityWebRequest.disposeUploadHandlerOnDispose; }
            set { UnityWebRequest.disposeUploadHandlerOnDispose = value; }
        }

        public ulong downloadedBytes => UnityWebRequest.downloadedBytes;


        public UnityEngine.Networking.DownloadHandler downloadHandler
        {
            get { return UnityWebRequest.downloadHandler; }
            set { UnityWebRequest.downloadHandler = value; }
        }

        public UnityEngine.Networking.UploadHandler uploadHandler
        {
            get { return UnityWebRequest.uploadHandler; }
            set { UnityWebRequest.uploadHandler = value; }
        }

        public float downloadProgress => UnityWebRequest.downloadProgress;

        public string error => UnityWebRequest.error;

        public bool isModifiable => UnityWebRequest.isModifiable;

        public string method
        {
            get { return UnityWebRequest.method; }
            set { UnityWebRequest.method = value; }
        }

        public int redirectLimit
        {
            get { return UnityWebRequest.redirectLimit; }
            set { UnityWebRequest.redirectLimit = value; }
        }

        public int timeout
        {
            get { return UnityWebRequest.timeout; }
            set { UnityWebRequest.timeout = value; }
        }

        public ulong uploadedBytes => UnityWebRequest.uploadedBytes;

        public Uri uri
        {
            get { return UnityWebRequest.uri; }
            set { UnityWebRequest.uri = value; }
        }

        public string url
        {
            get { return UnityWebRequest.url; }
            set { UnityWebRequest.url = value; }
        }

        public bool useHttpContinue
        {
            get { return UnityWebRequest.useHttpContinue; }
            set { UnityWebRequest.useHttpContinue = value; }
        }


        public bool isDone => UnityWebRequest.isDone;

        public long responseCode => UnityWebRequest.responseCode;

        public float uploadProgress => UnityWebRequest.uploadProgress;


    }

    [System.Serializable]
    public class RequestEvent : UnityEvent<BugsnagUnityWebRequest>
    {

    }

}
