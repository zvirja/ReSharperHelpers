using AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class RemoveFrozenAttributeActionTests : PathedContextActionExecuteTestBase<RemoveFrozenAttributeAction>
  {
    [Test] public void TestMatchingFrozenRemoved() { this.DoNamedTest2(); }
    [Test] public void TestSimpleFrozenRemoved() { this.DoNamedTest2(); }
  }

  public class RemoveFrozenAttributeActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<RemoveFrozenAttributeAction>
  {
    protected override string ExtraPath => nameof(RemoveFrozenAttributeActionTests);

    [Test] public void TestAvailability() { this.DoNamedTest2(); }
  }
}