using System.Linq;
using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.TextControl;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class AssertParametersNotNullActionTests : PathedContextActionExecuteTestBase<AssertParametersNotNullAction>
  {
    [Test] public void AssertAllWorksCorrectly() { this.DoNamedTest(); }

    [Test] public void AssertionPositionIsCorrect() { this.DoNamedTest(); }

    [Test] public void AssertStatementAndAnnotationAdded() { this.DoNamedTest(); }

    [Test] public void StringsAssertedCorrectly() { this.DoNamedTest(); }
  }

  public class AssertParametersNotNullActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<AssertParametersNotNullAction>
  {
    protected override string ExtraPath => nameof(AssertParametersNotNullActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }

  public class AssertParametersNotNullActionAssertAllAvailabilityTests : CSharpContextActionAvailabilityTestBase<AssertParametersNotNullAction>
  {
    protected override string ExtraPath => nameof(AssertParametersNotNullActionTests);

    protected override bool IsAvailable(AssertParametersNotNullAction action, ITextControl textControl)
    {
      var containerAvailable = base.IsAvailable(action, textControl);
      if (!containerAvailable) return false;

      var assertAllAction = action.CreateBulbItems().SingleOrDefault(b => b.BulbAction.Text == "Assert all parameters are not null");

      return assertAllAction != null;
    }

    [Test] public void AssertAllAvailability() { this.DoNamedTest(); }
  }
}