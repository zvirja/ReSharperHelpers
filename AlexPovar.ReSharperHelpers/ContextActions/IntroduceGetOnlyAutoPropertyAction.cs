using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Intentions.ContextActions;
using JetBrains.ReSharper.Intentions.QuickFixes.UsageChecking;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Create and Initialize private get-only auto-property.", Description = "Creates and Initializes private get-only auto-property.")]
  public class IntroduceGetOnlyAutoPropertyAction : InitializeActionBase, IContextAction
  {
    public IntroduceGetOnlyAutoPropertyAction(ICSharpContextActionDataProvider dataProvider)
      : base(dataProvider, CSharpLanguage.Instance)
    {
    }

    private IntroduceGetOnlyAutoPropertyFix CachedFix { get; set; }

    public override string Text => "[AlexHelpers] // SHOULD BE INVISIBLE";

    public new IEnumerable<IntentionAction> CreateBulbItems()
    {
      if (this.CachedFix == null) return Enumerable.Empty<IntentionAction>();

      var anchor = MyUtil.CreateGroupAnchor(IntentionsAnchors.ContextActionsAnchor);

      var actions = this.CachedFix.ToContextAction(anchor, MyIcons.ContextActionIcon);
      actions = actions.Concat(this.CachedFix.CreateAuxiliaryFix(AccessRights.PUBLIC, "Initialize {0}").ToContextAction(anchor, MyIcons.ContextActionIcon));

      return actions;
    }


    public override bool IsAvailable(IUserDataHolder cache)
    {
      return cache.GetData(InitializeAutoPropertyFix.InstanceKey) == null && base.IsAvailable(cache);
    }

    protected override IntroduceFromParameterFixBase CreateIntroduceFix(IParameter parameter)
    {
      return this.CachedFix ?? (this.CachedFix = this.MakeQuickFix(parameter, AccessRights.PRIVATE));
    }

    protected override IntroduceFromParameterFixBase CreateInitializeFix(IParameter parameter)
    {
      return this.CachedFix ?? (this.CachedFix = this.MakeQuickFix(parameter, AccessRights.PRIVATE));
    }

    private IntroduceGetOnlyAutoPropertyFix MakeQuickFix(IParameter parameter, AccessRights rights)
    {
      return new IntroduceGetOnlyAutoPropertyFix(parameter, rights)
      {
        TextFormat = "Initialize {0} get-only auto-property from parameter"
      };
    }
  }
}