using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class IntroduceGetOnlyAutoPropertyActionTests : PathedContextActionExecuteTestBase<IntroduceGetOnlyAutoPropertyAction>
  {
    [Test] public void AnnotationsAreCopied() { this.DoNamedTest(); }

    [Test] public void PrivatePropertyIntroduced() { this.DoNamedTest(); }

    [Test] public void PublicPropertyIntroduced() { this.DoNamedTest(); }
  }

  public class IntroduceGetOnlyAutoPropertyActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<IntroduceGetOnlyAutoPropertyAction>
  {
    protected override string ExtraPath => nameof(IntroduceGetOnlyAutoPropertyActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }
}