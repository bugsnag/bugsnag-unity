using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    class Breadcrumbs : IBreadcrumbs
    {
        readonly object _lock = new object();
        Configuration Configuration { get; }
        List<Breadcrumb> _breadcrumbs;

        /// <summary>
        /// Constructs a collection of breadcrumbs
        /// </summary>
        /// <param name="configuration"></param>
        internal Breadcrumbs(Configuration configuration)
        {
            Configuration = configuration;
            _breadcrumbs = new List<Breadcrumb>();
        }

        /// <summary>
        /// Add a breadcrumb to the collection with the specified type and metadata
        /// </summary>
        public void Leave(string message, Dictionary<string, object> metadata,BreadcrumbType type )
        {
            var breadcrumb = new Breadcrumb(message, metadata, type);
            Leave(breadcrumb);
        }

        public void Leave(Breadcrumb breadcrumb)
        {
            if (Configuration.MaximumBreadcrumbs == 0)
            {
                return;
            }

            if (breadcrumb != null)
            {
                lock (_lock)
                {

                    if (_breadcrumbs.Count == Configuration.MaximumBreadcrumbs)
                    {
                        _breadcrumbs.RemoveAt(0);
                    }

                    _breadcrumbs.Add(breadcrumb);
                }
            }
        }

        /// <summary>
        /// Retrieve the collection of breadcrumbs at this point in time.
        /// </summary>
        /// <returns></returns>
        public List<Breadcrumb> Retrieve()
        {
            lock (_lock)
            {
                return _breadcrumbs;
            }
        }

      
    }
}
