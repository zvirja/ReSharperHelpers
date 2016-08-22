namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.AssertParametersNotNullActionTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  class NamespaceIsImported
  {
    void Method(object ar{caret:Assert:parameter:is:not:null}g)
    {
    }
  }
}

namespace Diagnostics
{
  public static class Assert
  {
    public static void ArgumentNotNull<T>(T value, string argName);
    public static void ArgumentNotOrEmptyNull<T>(T value, string argName);
  }
}
