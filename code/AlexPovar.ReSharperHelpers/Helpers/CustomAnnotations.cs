using System;

namespace AlexPovar.ReSharperHelpers.Helpers
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
  public class CopyFromOriginalAttribute : Attribute
  {
    public CopyFromOriginalAttribute(string fullPath)
    {
    }
  }
}