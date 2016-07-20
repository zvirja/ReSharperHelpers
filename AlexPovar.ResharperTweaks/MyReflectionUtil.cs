using System;
using System.Reflection;

namespace AlexPovar.ResharperTweaks
{
  public static class MyReflectionUtil
  {
    public static T CreateStaticMethodInvocationDelegate<T>(Type type, string methodName) where T : class
    {
      var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
      if (methodInfo == null) throw new InvalidOperationException($"Unable to resolve method: {methodName}");

      return Delegate.CreateDelegate(typeof (T), methodInfo) as T;
    }

    public static T CreateInstanceMethodInvocationDelegate<T>(Type type, string methodName) where T : class
    {
      var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (methodInfo == null) throw new InvalidOperationException($"Unable to resolve method: {methodName}");

      return Delegate.CreateDelegate(typeof (T), methodInfo) as T;
    }
  }
}