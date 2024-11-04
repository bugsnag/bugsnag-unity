using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface IBreadcrumbs
    {
        void Leave(string message, Dictionary<string, object> metadata, BreadcrumbType type);

        void Leave(Breadcrumb breadcrumb);

        List<Breadcrumb> Retrieve();
    }
}
