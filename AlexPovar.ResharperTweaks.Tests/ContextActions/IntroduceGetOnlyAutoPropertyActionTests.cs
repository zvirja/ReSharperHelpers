using AlexPovar.ResharperTweaks.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ResharperTweaks.Tests.ContextActions
{
  public class IntroduceGetOnlyAutoPropertyActionTests : PathedContextActionExecuteTestBase<IntroduceGetOnlyAutoPropertyAction>
  {
    [TestCase("AnnotationsAreCopied.cs")]
    [TestCase("PrivatePropertyIntroduced.cs")]
    [TestCase("PublicPropertyIntroduced.cs")]
    public void RunIntroduceGetOnlyAutoPropertyActionTest(string testName)
    {
      this.DoTestFiles(testName);
    }
  }

  public class IntroduceGetOnlyAutoPropertyActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<IntroduceGetOnlyAutoPropertyAction>
  {
    protected override string ExtraPath => nameof(IntroduceGetOnlyAutoPropertyActionTests);

    [TestCase("Availability.cs")]
    public void RunIntroduceGetOnlyAutoPropertyActionAvailabilityTest(string testName)
    {
      this.DoTestFiles(testName);
    }
  }
}