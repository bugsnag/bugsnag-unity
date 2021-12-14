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
            NativeInterface.LeaveBreadcrumb(breadcrumb.Message, breadcrumb.Type.ToString(), breadcrumb.Metadata);
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
