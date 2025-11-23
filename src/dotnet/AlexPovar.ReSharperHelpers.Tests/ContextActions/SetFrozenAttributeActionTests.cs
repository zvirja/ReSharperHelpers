using System.Collections.Generic;
using AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.TextControl;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class SetFrozenAttributeActionTests : PathedContextActionExecuteTestBase<SetFrozenAttributeAction>
  {
    protected override IIntentionAction SelectActionToExecute(IReadOnlyList<IIntentionAction> intentionActions, string actionName, ITextControl textControl)
    {
      if (actionName == "Freeze")
      {
        actionName = "[AutoFixture] Freeze";
      }

      return base.SelectActionToExecute(intentionActions, actionName, textControl);
    }

    [Test] public void TestFrozenByExactTypeIsArranged() { this.DoNamedTest2(); }

    [Test] public void TestSimpleFrozenIsArranged() { this.DoNamedTest2(); }
  }

  public class SetFrozenAttributeActionAvailabilityTest : CSharpContextActionAvailabilityTestBase<SetFrozenAttributeAction>
  {
    protected override string ExtraPath => nameof(SetFrozenAttributeActionTests);

    [Test] public void TestAvailability1() { this.DoNamedTest2(); }

    [Test] public void TestAvailability2() { this.DoNamedTest2(); }
  }
}
