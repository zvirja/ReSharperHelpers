using System.Linq;
using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.TextControl;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class AssertParametersNotNullActionTests : PathedContextActionExecuteTestBase<AssertParametersNotNullAction>
  {
    [TestCase("AssertStatementAndAnnotationAdded.cs")]
    [TestCase("StringsAssertedCorrectly.cs")]
    [TestCase("AssertAllWorksCorrectly.cs")]
    public void Run(string testName)
    {
      this.DoTestFiles(testName);
    }
  }

  public class AssertParametersNotNullActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<AssertParametersNotNullAction>
  {
    protected override string ExtraPath => nameof(AssertParametersNotNullActionTests);

    [Test]
    public void Run()
    {
      this.DoTestFiles("Availability.cs");
    }
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

    [Test]
    public void Run()
    {
      this.DoTestFiles("AssertAllAvailability.cs");
    }
  }
}