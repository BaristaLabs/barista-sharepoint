namespace Barista.Helpers
{
  using System;
  using System.Reflection;
  using System.Linq;

  public static class DynamicHelper
  {
    public static T GetPropertyValue<T>(object obj, string propertyName)
    {
      var type = obj.GetType();
      var propertyInfo = type.GetProperty(propertyName, typeof (T));

      if (propertyInfo == null)
        throw new InvalidOperationException("The specified object did not contain a property named " + propertyName + " with the specified type.");

      return (T)propertyInfo.GetValue(obj, null);
    }

    public static void InvokeMethod(object obj, string methodName, params object[] parameters)
    {
      var type = obj.GetType();
      MethodInfo methodInfo;
      if (parameters == null || parameters.Any() == false)
      {
        methodInfo = type.GetMethod(methodName);
      }
      else
      {
        var parameterTypes = parameters.Select(p => p.GetType()).ToArray();
        methodInfo = type.GetMethod(methodName, parameterTypes);
      }

      if (methodInfo == null)
        throw new InvalidOperationException("The specified object did not contain a method named " + methodName + " with the specified parameter types.");

      methodInfo.Invoke(obj, parameters);
    }
  }
}
