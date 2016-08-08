using AlexPovar.ReSharperHelpers.ContextActions.Pure;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  //Folder name marker
  public static class PureNotPureActionsTests
  {
  }

  public abstract class PureActionTestBase<TContextAction> : CSharpContextActionExecuteTestBase<TContextAction> where TContextAction : class, IContextAction
  {
    protected override string ExtraPath => nameof(PureNotPureActionsTests);
  }

  public abstract class PureActionAvailabilityTestBase<TContextAction> : CSharpContextActionAvailabilityTestBase<TContextAction> where TContextAction : class, IContextAction
  {
    protected override string ExtraPath => nameof(PureNotPureActionsTests);
  }

  public class PureAttributeActionAvailabilityTests : PureActionAvailabilityTestBase<PureAttributeAction>
  {
    [Test] public void PureAvailability() { this.DoNamedTest(); }
  }

  public class NotPureAttributeActionAvailabilityTests : PureActionAvailabilityTestBase<NotPureAttributeAction>
  {
    [Test] public void NotPureAvailability() { this.DoNamedTest(); }
  }


  public class PureAttributeActionTests : PureActionTestBase<PureAttributeAction>
  {
    [Test] public void PureAttributeIsAdded() { this.DoNamedTest(); }

    [Test] public void PureAttributeIsMergedWithOther() { this.DoNamedTest(); }
  }

  public class NotPureAttributeActionTests : PureActionTestBase<NotPureAttributeAction>
  {
    [Test] public void PureAttributeIsRemoved() { this.DoNamedTest(); }
  }
}