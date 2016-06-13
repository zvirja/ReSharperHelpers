using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ResharperTweaks.ContextActions.ContainerNullability
{
  public class ItemCanBeNullAction : ContainerNullabilityActionBase
  {
    public ItemCanBeNullAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    public override string Text => "Item can be null";

    protected override string ThisAttributeShortName => ItemCanBeNullShortName;
  }
}