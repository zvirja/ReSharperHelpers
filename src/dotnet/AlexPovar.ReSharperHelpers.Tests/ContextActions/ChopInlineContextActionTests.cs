using AlexPovar.ReSharperHelpers.ContextActions.ChopInline;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class ChopInlineContextActionTests : PathedContextActionExecuteTestBase<ChopInlineContextAction>
  {
    [Test] public void MethodChoppedCorrectly() { this.DoNamedTest(); }

    [Test] public void MethodInlinedCorrectly() { this.DoNamedTest(); }
  }

  public class ChopInlineContextActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<ChopInlineContextAction>
  {
    protected override string ExtraPath => nameof(ChopInlineContextActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }
}