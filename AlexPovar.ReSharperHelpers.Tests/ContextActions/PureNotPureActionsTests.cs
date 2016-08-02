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
    [Test]
    public void Run()
    {
      this.DoTestFiles("PureAvailability.cs");
    }
  }

  public class NotPureAttributeActionAvailabilityTests : PureActionAvailabilityTestBase<NotPureAttributeAction>
  {
    [Test]
    public void Run()
    {
      this.DoTestFiles("NotPureAvailability.cs");
    }
  }


  public class PureAttributeActionTests : PureActionTestBase<PureAttributeAction>
  {
    [TestCase("PureAttributeIsAdded.cs")]
    [TestCase("PureAttributeIsMergedWithOther.cs")]
    public void Run(string testName)
    {
      this.DoTestFiles(testName);
    }
  }

  public class NotPureAttributeActionTests : PureActionTestBase<NotPureAttributeAction>
  {
    [TestCase("PureAttributeIsRemoved.cs")]
    public void Run(string testName)
    {
      this.DoTestFiles(testName);
    }
  }
}