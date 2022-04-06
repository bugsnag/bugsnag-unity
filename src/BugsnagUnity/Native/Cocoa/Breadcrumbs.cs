using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    class Breadcrumbs : IBreadcrumbs
    {
        internal Breadcrumbs()
        {
        }

        /// <summary>
        /// Add a breadcrumb to the collection with the specified type and metadata
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="metadata"></param>
        public void Leave(string message, Dictionary<string, object> metadata, BreadcrumbType type)
        {
            Leave(new Breadcrumb(message, metadata, type));
        }

        public void Leave(Breadcrumb breadcrumb)
        {
            if (breadcrumb == null || string.IsNullOrEmpty(breadcrumb.Message))
            {
                return;
            }

            string metadataJson = null;
            if (breadcrumb.Metadata != null)
            {
                using (var stream = new MemoryStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
                {
                    SimpleJson.SerializeObject(breadcrumb.Metadata, writer);
                    writer.Flush();
                    stream.Position = 0;
                    metadataJson = reader.ReadToEnd();
                }                    
            }
            NativeCode.bugsnag_addBreadcrumb(breadcrumb.Message, breadcrumb.Type.ToString().ToLowerInvariant(), metadataJson);
        }

        public List<Breadcrumb> Retrieve()
        {
            var breadcrumbs = new List<Breadcrumb>();

            var handle = GCHandle.Alloc(breadcrumbs);

            try
            {
                NativeCode.bugsnag_retrieveBreadcrumbs(GCHandle.ToIntPtr(handle), PopulateBreadcrumb);
            }
            finally
            {
                handle.Free();
            }

            return breadcrumbs;
        }

        [MonoPInvokeCallback(typeof(NativeCode.BreadcrumbInformation))]
        static void PopulateBreadcrumb(IntPtr instance, string message, string timestamp, string type, string metadataJson)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is List<Breadcrumb> breadcrumbs)
            {
                if (!string.IsNullOrEmpty(metadataJson))
                {
                    var metadata = ((JsonObject)SimpleJson.DeserializeObject(metadataJson)).GetDictionary();
                    breadcrumbs.Add(new Breadcrumb(message, timestamp, type, metadata));
                }
                else
                {
                    breadcrumbs.Add(new Breadcrumb(message, timestamp, type, null));
                }

                
            }
        }
    }
}
