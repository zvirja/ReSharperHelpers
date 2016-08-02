using AlexPovar.ReSharperHelpers.ContextActions.ChopInline;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class ChopInlineContextActionTests : PathedContextActionExecuteTestBase<ChopInlineContextAction>
  {
    [TestCase("MethodChoppedCorrectly.cs")]
    [TestCase("MethodInlinedCorrectly.cs")]
    public void Run(string testName)
    {
      this.DoTestFiles(testName);
    }
  }

  public class ChopInlineContextActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<ChopInlineContextAction>
  {
    protected override string ExtraPath => nameof(ChopInlineContextActionTests);

    [Test]
    public void Run()
    {
      this.DoTestFiles("Availability.cs");
    }
  }
}