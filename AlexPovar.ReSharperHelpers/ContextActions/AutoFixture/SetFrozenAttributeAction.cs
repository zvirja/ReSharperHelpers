using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Set Frozen AutoFixture attribute", Description = "Sets Frozen AutoFixture attribute.", Priority = short.MinValue)]
  public class SetFrozenAttributeAction : AutoFixtureAttributeAction
  {
    public SetFrozenAttributeAction([NotNull] ICSharpContextActionDataProvider provider)
      : base(provider, "Ploeh.AutoFixture.Xunit2.FrozenAttribute")
    {
    }

    public override string Text => "[AutoFixture] Freeze";
  }
}