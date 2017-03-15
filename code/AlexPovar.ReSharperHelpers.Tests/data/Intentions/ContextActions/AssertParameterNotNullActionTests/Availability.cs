using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.AssertParametersNotNullActionTests
{
  class Availability
  {
    void TestMethod(string string{on}arg, int int{off}arg, object obj{on}arg)
    {
    }
  }
}
