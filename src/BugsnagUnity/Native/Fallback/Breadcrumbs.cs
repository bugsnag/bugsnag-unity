using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    class Breadcrumbs : IBreadcrumbs
    {
        readonly object _lock = new object();
        IConfiguration Configuration { get; }
        Breadcrumb[] _breadcrumbs;
        int _current;
        BreadcrumbType[] EnabledBreadcrumbTypes;

        /// <summary>
        /// Constructs a collection of breadcrumbs
        /// </summary>
        /// <param name="configuration"></param>
        internal Breadcrumbs(IConfiguration configuration)
        {
            Configuration = configuration;
            _current = 0;
            _breadcrumbs = new Breadcrumb[Configuration.MaximumBreadcrumbs];
            EnabledBreadcrumbTypes = configuration.EnabledBreadcrumbTypes;
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
            if (breadcrumb != null)
            {
                lock (_lock)
                {
                    if (!IsBreadcrumbTypeEnabled(breadcrumb))
                    {
                        return;
                    }
                    var maximumBreadcrumbs = Configuration.MaximumBreadcrumbs;
                    if (_breadcrumbs.Length != maximumBreadcrumbs)
                    {
                        Array.Resize(ref _breadcrumbs, maximumBreadcrumbs);
                        if (_current >= maximumBreadcrumbs)
                        {
                            _current = 0;
                        }
                    }
                    _breadcrumbs[_current] = breadcrumb;
                    _current = (_current + 1) % maximumBreadcrumbs;
                }
            }
        }

        private bool IsBreadcrumbTypeEnabled(Breadcrumb breadcrumb)
        {
            if (EnabledBreadcrumbTypes == null)
            {
                return true;
            }
            foreach (var enabledType in EnabledBreadcrumbTypes)
            {
                var enabledTypeName = Enum.GetName(typeof(BreadcrumbType), enabledType).ToLower();
                if (breadcrumb.Type == enabledTypeName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieve the collection of breadcrumbs at this point in time.
        /// </summary>
        /// <returns></returns>
        public Breadcrumb[] Retrieve()
        {
            lock (_lock)
            {
                var numberOfBreadcrumbs = Array.IndexOf(_breadcrumbs, null);

                if (numberOfBreadcrumbs < 0) numberOfBreadcrumbs = _breadcrumbs.Length;

                var breadcrumbs = new Breadcrumb[numberOfBreadcrumbs];

                for (var i = 0; i < numberOfBreadcrumbs; i++)
                {
                    breadcrumbs[i] = _breadcrumbs[i];
                }

                return breadcrumbs;
            }
        }
    }
}
