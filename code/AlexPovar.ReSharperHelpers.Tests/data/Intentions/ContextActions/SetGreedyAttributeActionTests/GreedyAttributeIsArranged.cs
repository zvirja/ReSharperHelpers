using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.SetGreedyAttributeActionTests
{
  public class GreedyAttributeIsArranged
  {
    public void TestMethod(string some{caret}Arg)
    {
    }
  }
}

namespace Ploeh.AutoFixture.Xunit2
{
  public class GreedyAttribute
  {
  }
}
