using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  public class User : Dictionary<string, string>, IFilterable
  {
    internal User()
    {

    }

    public string Id
    {
      get
      {
        return this.Get("id");
      }
      set
      {
        this.AddToPayload("id", value);
      }
    }

    public string Name
    {
      get
      {
        return this.Get("name");
      }
      set
      {
        this.AddToPayload("name", value);
      }
    }

    public string Email
    {
      get
      {
        return this.Get("email");
      }
      set
      {
        this.AddToPayload("email", value);
      }
    }
  }
}
