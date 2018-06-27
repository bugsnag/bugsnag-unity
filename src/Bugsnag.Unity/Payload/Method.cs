using System.Linq;
using System.Reflection;
using System.Text;

namespace Bugsnag.Unity.Payload
{
  class Method
  {
    private readonly MethodBase _methodBase;

    internal Method(MethodBase methodBase)
    {
      _methodBase = methodBase;
    }

    internal string DisplayName()
    {
      if (_methodBase == null)
      {
        return null;
      }

      var builder = new StringBuilder();

      var type = _methodBase.DeclaringType;

      if (type != null)
      {
        var declaringTypeName = TypeNameHelper.GetTypeDisplayName(type, includeGenericParameterNames: true);
        if (!string.IsNullOrEmpty(declaringTypeName))
        {
          builder.Append(declaringTypeName).Append(".");
        }
      }

      builder.Append(_methodBase.Name);

      if (_methodBase.IsGenericMethod)
      {
        var genericArguments = string.Join(", ", _methodBase.GetGenericArguments()
          .Select(arg => TypeNameHelper.GetTypeDisplayName(arg, fullName: false, includeGenericParameterNames: true)).ToArray());
        builder.Append("<").Append(genericArguments).Append(">");
      }

      var parameters = _methodBase.GetParameters().Select(p => new MethodParameter(p).DisplayName()).ToArray();
      builder.Append("(");
      builder.Append(string.Join(", ", parameters));
      builder.Append(")");

      return builder.ToString();
    }
  }
}
