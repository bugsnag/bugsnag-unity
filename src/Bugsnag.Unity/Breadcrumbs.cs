using Bugsnag.Unity.Payload;
using System.Collections.Generic;

namespace Bugsnag.Unity
{
  public class Breadcrumbs
  {
    private readonly object _lock = new object();
    private readonly int _maximumBreadcrumbs;
    private readonly Breadcrumb[] _breadcrumbs;
    private int _current;

    /// <summary>
    /// Constructs a collection of breadcrumbs
    /// </summary>
    /// <param name="configuration"></param>
    public Breadcrumbs(Configuration configuration)
    {
      _maximumBreadcrumbs = configuration.MaximumBreadcrumbs;
      _current = 0;
      _breadcrumbs = new Breadcrumb[_maximumBreadcrumbs];
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
          _breadcrumbs[_current] = breadcrumb;
          _current = (_current + 1) % _maximumBreadcrumbs;
        }
      }
    }

    /// <summary>
    /// Retrieve the collection of breadcrumbs at this point in time.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Breadcrumb> Retrieve()
    {
      lock (_lock)
      {
        var numberOfBreadcrumbs = System.Array.IndexOf(_breadcrumbs, null);

        if (numberOfBreadcrumbs < 0) numberOfBreadcrumbs = _maximumBreadcrumbs;

        var breadcrumbs = new Breadcrumb[numberOfBreadcrumbs];

        for (int i = 0; i < numberOfBreadcrumbs; i++)
        {
          breadcrumbs[i] = _breadcrumbs[i];
        }

        return breadcrumbs;
      }
    }
  }
}
