namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.SetGreedyAttributeActionTests
{
  using AutoFixture.Xunit2;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public class Availabi{off}lity1
  {
    public void Te{off}st(string some{on}arg, [Greedy] string other{off}Arg)
    {
      Action<string> act => (va{off}lue) => {};

      void InnerMethod1(string val{off}ue)
      {
      }

      static void InnerMethod2(string val{off}ue)
      {
      }
    }
  }
}

namespace AutoFixture.Xunit2
{
  using System;

  public class GreedyAttribute: Attribute
  {
  }
}
