using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;

namespace AlexPovar.ResharperTweaks.Tests
{
  public abstract class PathedContextActionExecuteTestBase<TContextAction> : CSharpContextActionExecuteTestBase<TContextAction> where TContextAction : class, IContextAction
  {
    protected override string ExtraPath => this.GetType().Name;
  }

  public abstract class PathedCSharpContextActionAvailabilityTestBase<TContextAction> : CSharpContextActionAvailabilityTestBase<TContextAction> where TContextAction : class, IContextAction
  {
    protected override string ExtraPath => this.GetType().Name;
  }
}