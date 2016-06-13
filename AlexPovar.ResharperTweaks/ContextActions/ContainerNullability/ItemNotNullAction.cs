using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ResharperTweaks.ContextActions.ContainerNullability
{
  public class ItemNotNullAction : ContainerNullabilityActionBase
  {
    public ItemNotNullAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    public override string Text => "Item not null";

    protected override string ThisAttributeShortName => ItemNotNullShortName;
  }
}