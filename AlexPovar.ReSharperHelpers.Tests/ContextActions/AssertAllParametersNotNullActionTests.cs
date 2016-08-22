using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class AssertAllParametersNotNullActionTests : PathedContextActionExecuteTestBase<AssertAllParametersNotNullAction>
  {
    [Test] public void OrderedForNotResolved() { this.DoNamedTest(); }

    [Test] public void WorksCorrectly() { this.DoNamedTest(); }
  }

  public class AssertParametersNotNullActionAssertAllAvailabilityTests : CSharpContextActionAvailabilityTestBase<AssertAllParametersNotNullAction>
  {
    protected override string ExtraPath => nameof(AssertAllParametersNotNullActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }
}