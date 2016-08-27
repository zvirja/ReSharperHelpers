using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Set Greedy AutoFixture attribute", Description = "Sets Greedy AutoFixture attribute.", Priority = short.MinValue)]
  public class SetGreedyAttribute : AutoFixtureAttributeActionBase
  {
    public SetGreedyAttribute([NotNull] ICSharpContextActionDataProvider provider)
      : base(provider, AutoFixtureConstants.GreedyAttributeType)
    {
    }

    public override string Text => "[AutoFixture] Use greedy constructor";
  }
}