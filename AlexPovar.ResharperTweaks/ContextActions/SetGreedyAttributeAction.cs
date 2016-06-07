using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Set Greedy AutoFixture attribute",
    Description = "Sets Greedy AutoFixture attribute.",
    Priority = short.MinValue)]
  public class SetGreedyAttributeAction : AutoFixtureAttributeAction
  {
    public SetGreedyAttributeAction([NotNull] ICSharpContextActionDataProvider provider)
      : base(provider, "Ploeh.AutoFixture.Xunit2.GreedyAttribute")
    {
    }

    public override string Text => "[AutoFixture] Use greedy constructor";
  }
}