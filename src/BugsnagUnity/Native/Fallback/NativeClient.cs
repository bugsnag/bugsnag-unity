﻿using BugsnagUnity.Payload;
using System.Collections.Generic;

namespace BugsnagUnity
{
  class NativeClient : INativeClient
  {
    public IConfiguration Configuration { get; }

    public IBreadcrumbs Breadcrumbs { get; }

    public IDelivery Delivery { get; }

    public NativeClient(Configuration configuration)
    {
      Configuration = configuration;
      Breadcrumbs = new Breadcrumbs(configuration);
      Delivery = new Delivery();
    }

    public void PopulateApp(App app)
    {
    }

    public void PopulateDevice(Device device)
    {
    }

    public void PopulateMetadata(Metadata metadata)
    {
    }

    public void PopulateUser(User user)
    {
    }

    public void SetMetadata(string tab, Dictionary<string, string> metadata)
    {
    }

    public void SetSession(Session session)
    {
    }

    public void SetUser(User user)
    {
    }
  }
}
