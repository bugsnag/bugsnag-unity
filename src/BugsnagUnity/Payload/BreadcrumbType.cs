namespace BugsnagUnity.Payload
{
  /// <summary>
  /// Represents all of the possible breadcrumb types that the Bugsnag API supports.
  /// </summary>
  public enum BreadcrumbType
  {
    /// <summary>
    /// A breadcrumb with navigation information.
    /// </summary>
    Navigation,

    /// <summary>
    /// A breadcrumb with request information.
    /// </summary>
    Request,

    /// <summary>
    /// A breadcrumb with process information.
    /// </summary>
    Process,

    /// <summary>
    /// A breadcrumb with log information.
    /// </summary>
    Log,

    /// <summary>
    /// A breadcrumb with user information.
    /// </summary>
    User,

    /// <summary>
    /// A breadcrumb with state information.
    /// </summary>
    State,

    /// <summary>
    /// A breadcrumb with error information.
    /// </summary>
    Error,

    /// <summary>
    /// A manually logged breadcrumb.
    /// </summary>
    Manual,
  }
}
