using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Set Frozen AutoFixture attribute", Description = "Sets Frozen AutoFixture attribute.", Priority = short.MinValue)]
  public class SetFrozenAttribute : AutoFixtureAttributeActionBase
  {
    public SetFrozenAttribute([NotNull] ICSharpContextActionDataProvider provider)
      : base(provider, AutoFixtureConstants.FrozenAttributeType)
    {
    }

    public override string Text => "[AutoFixture] Freeze";
  }
}