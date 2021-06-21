using System;
namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents all of the possible breadcrumb types that the Bugsnag API supports.
    /// </summary>
    [Serializable]
    [Flags]
    public enum BreadcrumbType
    {
        /// <summary>
        /// A breadcrumb with navigation information.
        /// </summary>
        Navigation = 0,

        /// <summary>
        /// A breadcrumb with request information.
        /// </summary>
        Request = 1,

        /// <summary>
        /// A breadcrumb with process information.
        /// </summary>
        Process = 2,

        /// <summary>
        /// A breadcrumb with log information.
        /// </summary>
        Log = 3,

        /// <summary>
        /// A breadcrumb with user information.
        /// </summary>
        User = 4,

        /// <summary>
        /// A breadcrumb with state information.
        /// </summary>
        State = 5,

        /// <summary>
        /// A breadcrumb with error information.
        /// </summary>
        Error = 6,

        /// <summary>
        /// A manually logged breadcrumb.
        /// </summary>
        Manual = 7,
    }
}
