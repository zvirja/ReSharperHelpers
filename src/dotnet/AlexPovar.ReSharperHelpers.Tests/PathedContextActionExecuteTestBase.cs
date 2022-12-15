using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;

namespace AlexPovar.ReSharperHelpers.Tests
{
  public abstract class PathedContextActionExecuteTestBase<TContextAction> : CSharpContextActionExecuteTestBase<TContextAction> where TContextAction : class, IContextAction
  {
    protected override string ExtraPath => this.GetType().Name;
  }
}