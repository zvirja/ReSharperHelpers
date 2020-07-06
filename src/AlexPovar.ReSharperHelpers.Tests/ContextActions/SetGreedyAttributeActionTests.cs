using AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class SetGreedyAttributeActionTests : PathedContextActionExecuteTestBase<SetGreedyAttributeAction>
  {
    [Test] public void TestGreedyAttributeIsArranged() { this.DoNamedTest2(); }
  }

  public class SetGreedyAttributeActionAvailabilityTests1 : CSharpContextActionAvailabilityTestBase<SetGreedyAttributeAction>
  {
    protected override string ExtraPath => nameof(SetGreedyAttributeActionTests);

    [Test] public void TestAvailability1() { this.DoNamedTest2(); }

    [Test] public void TestAvailability2() { this.DoNamedTest2(); }
  }
}