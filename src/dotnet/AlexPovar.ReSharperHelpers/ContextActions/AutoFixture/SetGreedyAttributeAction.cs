using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;

[ContextAction(Group = "C#", Name = "[ReSharperHelpers] Set Greedy AutoFixture attribute", Description = "Sets Greedy AutoFixture attribute.", Priority = short.MinValue)]
public class SetGreedyAttributeAction : AutoFixtureAttributeActionBase
{
  public SetGreedyAttributeAction([NotNull] ICSharpContextActionDataProvider provider)
    : base(provider, AutoFixtureConstants.GreedyAttributeType)
  {
  }

  public override string Text => "[AutoFixture] Use greedy constructor";
}
