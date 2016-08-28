using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Remove Greedy AutoFixture attribute", Description = "Removes Greedy AutoFixture attribute.", Priority = short.MinValue)]
  public class RemoveGreedyAttribute : AutoFixtureAttributeActionBase
  {
    public RemoveGreedyAttribute([NotNull] ICSharpContextActionDataProvider provider) : base(provider, AutoFixtureConstants.GreedyAttributeType)
    {
    }

    public override string Text => "Remove [Greedy] attribute";


    protected override bool IsAvailableWithAttributeInstances(IList<IAttributeInstance> existingAttributes)
    {
      return !existingAttributes.IsEmpty();
    }

    //Remove existing, but don't create a new one.
    protected override IAttribute CreateAttribute(ITypeElement resolvedAttributeType, CSharpElementFactory factory, IPsiModule psiModule) => null;
  }
}