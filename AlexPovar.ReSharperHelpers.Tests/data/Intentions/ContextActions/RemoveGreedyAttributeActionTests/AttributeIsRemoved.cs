using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.RemoveGreedyAttributeActionTests
{
  public class AttributeIsRemoved
  {
    public void TestMethod([Greedy] string some{caret}Arg) { }
  }
}

namespace Ploeh.AutoFixture.Xunit2
{
  public class GreedyAttribute: Attribute
  {
  }
}