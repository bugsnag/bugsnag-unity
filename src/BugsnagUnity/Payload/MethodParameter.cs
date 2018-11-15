using System.Reflection;
using System.Text;

namespace BugsnagUnity.Payload
{
  class MethodParameter
  {
    private readonly ParameterInfo _originalParameterInfo;

    internal MethodParameter(ParameterInfo parameterInfo)
    {
      _originalParameterInfo = parameterInfo;
    }

    internal string DisplayName()
    {
      var builder = new StringBuilder();
      var type = _originalParameterInfo.ParameterType;

      if (_originalParameterInfo.IsOut)
      {
        builder.Append("out ");
      }
      else if (type != null && type.IsByRef)
      {
        builder.Append("ref ");
      }

      var parameterTypeString = "?";

      if (type != null)
      {
        if (type.IsByRef)
        {
          type = type.GetElementType();
        }

        parameterTypeString = TypeNameHelper.GetTypeDisplayName(type, fullName: false, includeGenericParameterNames: true);
      }

      builder.Append(parameterTypeString).Append(" ").Append(_originalParameterInfo.Name);

      return builder.ToString();
    }
  }
}
