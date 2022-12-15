using AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class RemoveGreedyAttributeActionTests : PathedContextActionExecuteTestBase<RemoveGreedyAttributeAction>
  {
    [Test] public void TestAttributeIsRemoved() { this.DoNamedTest2(); }
  }

  public class RemoveGreedyAttributeActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<RemoveGreedyAttributeAction>
  {
    protected override string ExtraPath => nameof(RemoveGreedyAttributeActionTests);

    [Test] public void TestAvailability() { this.DoNamedTest2(); }
  }
}