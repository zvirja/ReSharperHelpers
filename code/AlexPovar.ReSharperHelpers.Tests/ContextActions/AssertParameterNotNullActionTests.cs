using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class AssertParameterNotNullActionTests : PathedContextActionExecuteTestBase<AssertParameterNotNullAction>
  {
    [Test] public void TestAssertionPositionIsCorrect() { this.DoNamedTest2(); }

    [Test] public void TestAssertStatementAndAnnotationAdded() { this.DoNamedTest2(); }

    [Test] public void TestCorrectOverloadIsSelected() { this.DoNamedTest2(); }

    [Test] public void TestNamespaceIsImported() { this.DoNamedTest2(); }

    [Test] public void TestStringsAssertedCorrectly() { this.DoNamedTest2(); }
  }

  public class AssertParametersNotNullActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<AssertParameterNotNullAction>
  {
    protected override string ExtraPath => nameof(AssertParameterNotNullActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }
}