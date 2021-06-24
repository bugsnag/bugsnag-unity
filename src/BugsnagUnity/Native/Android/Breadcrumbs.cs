using System;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity.Payload;

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
        /// Add a breadcrumb to the collection using Manual type and no metadata.
        /// </summary>
        /// <param name="message"></param>
        public void Leave(string message)
        {
            Leave(message, BreadcrumbType.Manual, null);
        }

        /// <summary>
        /// Add a breadcrumb to the collection with the specified type and metadata
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="metadata"></param>
        public void Leave(string message, BreadcrumbType type, IDictionary<string, string> metadata)
        {
            Leave(new Breadcrumb(message, type, metadata));
        }

        /// <summary>
        /// Add a pre assembled breadcrumb to the collection.
        /// </summary>
        /// <param name="breadcrumb"></param>
        public void Leave(Breadcrumb breadcrumb)
        {
            NativeInterface.LeaveBreadcrumb(breadcrumb.Name, breadcrumb.Type, breadcrumb.Metadata);
        }

        /// <summary>
        /// Retrieve the collection of breadcrumbs at this point in time.
        /// </summary>
        /// <returns></returns>
        public Breadcrumb[] Retrieve()
        {
            return NativeInterface.GetBreadcrumbs().ToArray();
        }

    }
}
