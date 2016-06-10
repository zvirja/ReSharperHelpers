using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ResharperTweaks.ContextActions.AutoFixture
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Set Frozen AutoFixture attribute",
    Description = "Sets Frozen AutoFixture attribute.",
    Priority = short.MinValue)]
  public class SetFrozenAttributeAction : AutoFixtureAttributeAction
  {
    public SetFrozenAttributeAction([NotNull] ICSharpContextActionDataProvider provider)
      : base(provider, "Ploeh.AutoFixture.Xunit2.FrozenAttribute")
    {
    }

    public override string Text => "[AutoFixture] Freeze";
  }
}