using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            if (breadcrumb != null)
            {
                if (breadcrumb.Metadata != null)
                {
                    var index = 0;
                    var metadata = new string[breadcrumb.Metadata.Count * 2];

                    foreach (var data in breadcrumb.Metadata)
                    {
                        metadata[index] = data.Key;
                        metadata[index + 1] = data.Value.ToString();
                        index += 2;
                    }

                    NativeCode.bugsnag_addBreadcrumb(breadcrumb.Name, breadcrumb.Type, metadata, metadata.Length);
                }
                else
                {
                    NativeCode.bugsnag_addBreadcrumb(breadcrumb.Name, breadcrumb.Type, null, 0);
                }
            }
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
        static void PopulateBreadcrumb(IntPtr instance, string name, string timestamp, string type, string[] keys, int keysSize, string[] values, int valuesSize)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is List<Breadcrumb> breadcrumbs)
            {
                var metadata = new Dictionary<string, object>();

                for (var i = 0; i < keys.Length; i++)
                {
                    metadata.Add(keys[i], values[i]);
                }

                breadcrumbs.Add(new Breadcrumb(name, timestamp, type, metadata));
            }
        }
    }
}
