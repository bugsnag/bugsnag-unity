using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
  public class AsyncOperation
  {
    public event Action<AsyncOperation> completed;
  }

  public class UnityWebRequestAsyncOperation : AsyncOperation
  {

  }
}
