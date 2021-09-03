using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Report : Dictionary<string, object>, IPayload
    {
        /// <summary>
        /// Gets the configuration used to construct this report.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the endpoint that will be used to send the report to.
        /// </summary>
        /// <value>The endpoint.</value>
        public Uri Endpoint => Configuration.Endpoints.Notify;

        /// <summary>
        /// Gets the headers that will be attached to the http request used to send
        /// this report.
        /// </summary>
        /// <value>The headers.</value>
        public KeyValuePair<string, string>[] Headers { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:BugsnagUnity.Payload.Report"/>
        /// is ignored. If ignored the report will not be sent to Bugsnag.
        /// </summary>
        /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
        public bool Ignored { get; private set; }

        internal Report(IConfiguration configuration, Event @event)
        {
            Ignored = false;
            Configuration = configuration;
            Headers = new[] {
        new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
        new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.PayloadVersion),
      };
            Event = @event;
            this.AddToPayload("apiKey", configuration.ApiKey);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("events", new[] { Event });
        }

        Event Event { get; }

        internal HandledState OriginalSeverity => Event.OriginalSeverity;

        /// <summary>
        /// Ignore this report and do not send it to Bugsnag.
        /// </summary>
        public void Ignore()
        {
            Ignored = true;
        }

        /// <summary>
        /// Gets the metadata associated with this report.
        /// </summary>
        /// <value>The metadata.</value>
        public Metadata Metadata => Event.Metadata;

        /// <summary>
        /// Gets the Unity LogType that this event was generated with. Will be null
        /// if the report was not generated from a Unity log message.
        /// </summary>
        /// <value>The type of the log.</value>
        public LogType? LogType => Event.LogType;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:BugsnagUnity.Payload.Report"/>
        /// is a handled error or not.
        /// </summary>
        /// <value><c>true</c> if is handled; otherwise, <c>false</c>.</value>
        public bool IsHandled => Event.IsHandled;

        /// <summary>
        /// Gets the app data that was gathered for this report.
        /// </summary>
        /// <value>The app.</value>
        public App App => Event.App;

        /// <summary>
        /// Gets the session data that was gathered for this report.
        /// </summary>
        /// <value>The session.</value>
        public Session Session => Event.Session;

        /// <summary>
        /// Gets the breadcrumbs that have been attached to this report.
        /// </summary>
        /// <value>The breadcrumbs.</value>
        public IEnumerable<Breadcrumb> Breadcrumbs => Event.Breadcrumbs;

        /// <summary>
        /// Gets or sets the context for this report.
        /// </summary>
        /// <value>The context.</value>
        public string Context
        {
            get => Event.Context;
            set => Event.Context = value;
        }

        /// <summary>
        /// Gets the device data that was gathered for this report.
        /// </summary>
        /// <value>The device.</value>
        public Device Device => Event.Device;

        /// <summary>
        /// Gets the exceptions that generated this report. There may be multiple of
        /// these if the exception had inner exceptions.
        /// </summary>
        /// <value>The exceptions.</value>
        public Exception[] Exceptions => Event.Exceptions;

        /// <summary>
        /// Gets or sets the grouping hash for this report. Can be used to provide
        /// your own grouping logic.
        /// </summary>
        /// <value>The grouping hash.</value>
        public string GroupingHash
        {
            get => Event.GroupingHash;
            set => Event.GroupingHash = value;
        }

        /// <summary>
        /// Gets or sets the severity of this report.
        /// </summary>
        /// <value>The severity.</value>
        public Severity Severity
        {
            get => Event.Severity;
            set => Event.Severity = value;
        }

        /// <summary>
        /// Gets or sets the user information associated with this report.
        /// </summary>
        /// <value>The user.</value>
        public User User
        {
            get => Event.User;
            set => Event.User = value;
        }
    }
}
