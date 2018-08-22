using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
  public class Report : Dictionary<string, object>, IPayload
  {
    public IConfiguration Configuration { get; }

    public Uri Endpoint => Configuration.Endpoint;

    public KeyValuePair<string, string>[] Headers { get; }

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

    public void Ignore()
    {
      Ignored = true;
    }

    public Metadata Metadata => Event.Metadata;

    public LogType? LogType => Event.LogType;

    public bool IsHandled => Event.IsHandled;

    public App App => Event.App;

    public IEnumerable<Breadcrumb> Breadcrumbs => Event.Breadcrumbs;

    public string Context
    {
      get => Event.Context;
      set => Event.Context = value;
    }

    public Device Device => Event.Device;

    public Exception[] Exceptions => Event.Exceptions;
    
    public string GroupingHash
    {
      get => Event.GroupingHash;
      set => Event.GroupingHash = value;
    }
    
    public Severity Severity
    {
      get => Event.Severity;
      set => Event.Severity = value;
    }
    
    public User User
    {
      get => Event.User;
      set => Event.User = value;
    }
  }
}
