namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.RemoveGreedyAttributeActionTests
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public class Availability
  {
    public void TestMethod(string arg{off}1, [AutoFixture.Xunit2.Greedy] string arg{on}2) { }

    public void TestMethod2([Greedy] string some{off}arg) { }
  }

  public class GreedyAttribute: Attribute { }
}

namespace AutoFixture.Xunit2
{
  using System;

  public class GreedyAttribute : Attribute
  {
  }
}