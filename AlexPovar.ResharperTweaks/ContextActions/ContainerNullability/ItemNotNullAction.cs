using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ResharperTweaks.ContextActions.ContainerNullability
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Set ItemNotNull attribute", Description = "Sets ItemNotNull annotation attribute.")]
  public class ItemNotNullAction : ContainerNullabilityActionBase
  {
    public ItemNotNullAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    public override string Text => "Item not null";

    protected override string ThisAttributeShortName => ItemNotNullShortName;
  }
}