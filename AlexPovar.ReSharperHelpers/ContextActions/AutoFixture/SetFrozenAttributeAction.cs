using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Set Frozen AutoFixture attribute", Description = "Sets Frozen AutoFixture attribute.", Priority = short.MinValue)]
  public class SetFrozenAttributeAction : AutoFixtureAttributeActionBase
  {
    [NotNull] private static readonly string[] MatchingFlags =
    {
      "ExactType",
      "DirectBaseType",
      "ImplementedInterfaces",
      "ParameterName",
      "PropertyName",
      "FieldName",
      "MemberName"
    };

    public SetFrozenAttributeAction([NotNull] ICSharpContextActionDataProvider provider) : this(provider, null)
    {
    }

    private SetFrozenAttributeAction([NotNull] ICSharpContextActionDataProvider provider, [CanBeNull] string matchingFlagName)
      : base(provider, AutoFixtureConstants.FrozenAttributeType)
    {
      this.MatchingFlagName = matchingFlagName;
    }

    [CanBeNull]
    private string MatchingFlagName { get; }

    public override string Text => this.MatchingFlagName == null ? "[AutoFixture] Freeze" : $"Matching {this.MatchingFlagName}";

    protected override IAttribute CreateAttribute(ITypeElement resolvedAttributeType, CSharpElementFactory factory, IPsiModule psiModule)
    {
      if (this.MatchingFlagName == null) return base.CreateAttribute(resolvedAttributeType, factory, psiModule);

      var enumType = TypeElementUtil.GetTypeElementByClrName(AutoFixtureConstants.MathingEnumType, psiModule) as IEnum;
      var enumValue = enumType?.EnumMembers.FirstOrDefault(f => f.ShortName == this.MatchingFlagName)?.ConstantValue;

      if (enumValue == null) return null;

      return factory.CreateAttribute(resolvedAttributeType, new[] {new AttributeValue(enumValue)}, EmptyArray<Pair<string, AttributeValue>>.Instance);
    }


    public override IEnumerable<IntentionAction> CreateBulbItems()
    {
      var anchor = MyUtil.CreateHelperActionsGroupAnchor();
      yield return this.ToHelpersContextActionIntention(anchor);

      foreach (var matchingFlag in MatchingFlags)
      {
        yield return new SetFrozenAttributeAction(this.Provider, matchingFlag).ToHelpersContextActionIntention(anchor);
      }
    }
  }
}