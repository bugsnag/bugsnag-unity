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
        public Configuration Configuration { get; }

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
        internal bool Ignored { get; private set; }

        internal Report(Configuration configuration, Event @event)
        {
            Ignored = false;
            Configuration = configuration;
            Headers = new[] {
        new KeyValuePair<string, string>("Bugsnag-Api-Key", Configuration.ApiKey),
        new KeyValuePair<string, string>("Bugsnag-Payload-Version", Configuration.PayloadVersion),
      };
            _event = @event;
            this.AddToPayload("apiKey", configuration.ApiKey);
            this.AddToPayload("notifier", NotifierInfo.Instance);
            this.AddToPayload("events", new[] { _event.GetEventPayload() });
        }

        private Event _event;

        /// <summary>
        /// Gets the session data that was gathered for this report.
        /// </summary>
        /// <value>The session.</value>
        internal Session Session => _event.Session;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:BugsnagUnity.Payload.Report"/>
        /// is a handled error or not.
        /// </summary>
        /// <value><c>true</c> if is handled; otherwise, <c>false</c>.</value>
        internal bool IsHandled => _event.IsHandled;


        internal string Context => _event.Context;

        /// <summary>
        /// Gets the exceptions that generated this report. There may be multiple of
        /// these if the exception had inner exceptions.
        /// </summary>
        /// <value>The exceptions.</value>
        internal List<Exception> Exceptions => _event.Exceptions;

        internal HandledState OriginalSeverity => _event.OriginalSeverity;

        /// <summary>
        /// Ignore this report and do not send it to Bugsnag.
        /// </summary>
        public void Ignore()
        {
            Ignored = true;
        }

    }
}
