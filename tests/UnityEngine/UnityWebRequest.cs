using System;

namespace UnityEngine.Networking
{
    public class UnityWebRequest
    {
        public UnityWebRequest(Uri uri, string method)
        {

        }

        //
        // Summary:
        //     Set a HTTP request header to a custom value.
        //
        // Parameters:
        //   name:
        //     The key of the header to be set. Case-sensitive.
        //
        //   value:
        //     The header's intended value.
        public void SetRequestHeader(string name, string value)
        {

        }

        //
        // Summary:
        //     Holds a reference to the UploadHandler object which manages body data to be uploaded
        //     to the remote server.
        public UploadHandler uploadHandler { get; set; }
        //
        // Summary:
        //     Holds a reference to a DownloadHandler object, which manages body data received
        //     from the remote server by this UnityWebRequest.
        public DownloadHandler downloadHandler { get; set; }

        //
        // Summary:
        //     Begin communicating with the remote server. After calling this method, the UnityWebRequest
        //     will perform DNS resolution (if necessary), transmit an HTTP request to the remote
        //     server at the target URL and process the server’s response. This method can only
        //     be called once on any given UnityWebRequest object. Once this method is called,
        //     you cannot change any of the UnityWebRequest’s properties. This method returns
        //     a WebRequestAsyncOperation object. Yielding the WebRequestAsyncOperation inside
        //     a coroutine will cause the coroutine to pause until the UnityWebRequest encounters
        //     a system error or finishes communicating.
        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            throw new NotImplementedException();
        }
    }
}
