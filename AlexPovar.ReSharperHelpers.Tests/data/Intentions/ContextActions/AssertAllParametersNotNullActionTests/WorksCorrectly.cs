using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.AssertAllParametersNotNullActionTests
{
  class WorksCorrectly
  {
    void TestMethod(object ar{caret}g1, int arg2, string arg3)
    {
    }
  }

  public static class Assert
  {
    public static void ArgumentNotNull<T>(T value, string argName);
    public static void ArgumentNotOrEmptyNull<T>(T value, string argName);
  }
}