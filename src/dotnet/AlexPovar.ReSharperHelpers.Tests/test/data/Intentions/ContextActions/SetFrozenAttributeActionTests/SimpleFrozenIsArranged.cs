using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.SetFrozenAttributeActionTests
{
  public class SimpleFrozenIsArranged
  {
    public void TestMethod(string some{caret:Freeze}Arg) { }
  }
}


namespace AutoFixture.Xunit2
{
  using System;

  public class FrozenAttribute : Attribute
  {
    public FrozenAttribute() { }

    public FrozenAttribute(Matching by) { }
  }

  public enum Matching
  {
    ExactType = 1,
    DirectBaseType = 2,
    ImplementedInterfaces = 4,
    ParameterName = 8,
    PropertyName = 16,
    FieldName = 32,
    MemberName = FieldName | PropertyName | ParameterName
  }
}

