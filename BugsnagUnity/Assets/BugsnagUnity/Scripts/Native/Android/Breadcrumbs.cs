#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity.Payload;
using System.Linq;

namespace BugsnagUnity
{
    class Breadcrumbs : IBreadcrumbs
    {
        NativeInterface NativeInterface { get; }

        internal Breadcrumbs(NativeInterface nativeInterface)
        {
            NativeInterface = nativeInterface;
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

        /// <summary>
        /// Add a pre assembled breadcrumb to the collection.
        /// </summary>
        /// <param name="breadcrumb"></param>
        public void Leave(Breadcrumb breadcrumb)
        {
            if (breadcrumb == null || string.IsNullOrEmpty(breadcrumb.Message))
            {
                return;
            }
            // Clone the metadata to prevent thread related exceptions
            var metadataClone = breadcrumb.Metadata.ToDictionary(entry => entry.Key, entry => entry.Value);
            NativeInterface.LeaveBreadcrumb(breadcrumb.Message, breadcrumb.Type.ToString(), metadataClone);
        }

        /// <summary>
        /// Retrieve the collection of breadcrumbs at this point in time.
        /// </summary>
        /// <returns></returns>
        public List<Breadcrumb> Retrieve()
        {
            return NativeInterface.GetBreadcrumbs();
        }

    }
}
#endif