using System.Collections.Generic;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  public interface IBreadcrumbs
  {
    void Leave(string message);

    void Leave(string message, BreadcrumbType type, IDictionary<string, string> metadata);

    void Leave(Breadcrumb breadcrumb);

    Breadcrumb[] Retrieve();
  }
}
