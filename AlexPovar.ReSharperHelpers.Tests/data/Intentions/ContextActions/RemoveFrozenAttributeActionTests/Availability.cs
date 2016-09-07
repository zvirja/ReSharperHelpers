namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.RemoveFrozenAttributeActionTests
{
  using Ploeh.AutoFixture.Xunit2;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public class Availability
  {
    public void TestMethod([Frozen] object ar{on}g1,[Frozen(Matching.PropertyName)] object ar{on}g2, object ar{off}g3) { }
  }
}


namespace Ploeh.AutoFixture.Xunit2
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

