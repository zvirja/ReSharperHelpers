using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class AssertParameterNotNullActionTests : PathedContextActionExecuteTestBase<AssertParameterNotNullAction>
  {
    [Test] public void AssertionPositionIsCorrect() { this.DoNamedTest(); }

    [Test] public void AssertStatementAndAnnotationAdded() { this.DoNamedTest(); }

    [Test] public void NamespaceIsImported() { this.DoNamedTest(); }

    [Test] public void StringsAssertedCorrectly() { this.DoNamedTest(); }
  }

  public class AssertParametersNotNullActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<AssertParameterNotNullAction>
  {
    protected override string ExtraPath => nameof(AssertParameterNotNullActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }
}