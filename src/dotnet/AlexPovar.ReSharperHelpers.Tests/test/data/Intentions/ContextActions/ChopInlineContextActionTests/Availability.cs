using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.ChopInlineContextActionTests
{
  class Availability
  {
    public Availa{off}bility()
    {
    }

    public Availa{on}bility(int arg1)
    {
    }

    void MethodWithout{off}Args()
    {
    }

    void MethodWith{on}SingleArg(string arg1)
    {
    }

    void MethodWith{on}Args(int arg1, string arg2)
    {
    }
  }
}
