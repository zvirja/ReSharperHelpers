using AlexPovar.ResharperTweaks.QuickFixes;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Intentions.ContextActions;
using JetBrains.ReSharper.Intentions.QuickFixes.UsageChecking;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Create and Initialize private get-only auto-property.", Description = "Creates and Initializes private get-only auto-property.")]
  public class IntroducePrivateGetOnlyAutoPropertyAction : InitializeActionBase
  {
    public IntroducePrivateGetOnlyAutoPropertyAction(ICSharpContextActionDataProvider dataProvider)
      : base(dataProvider, CSharpLanguage.Instance)
    {
    }

    public override string Text => "Initialize private get-only auto-property from parameter";

    protected override IntroduceFromParameterFixBase CreateIntroduceFix(IParameter parameter)
    {
      return new IntroducePrivateGetOnlyAutoPropertyFix(parameter);
    }

    protected override IntroduceFromParameterFixBase CreateInitializeFix(IParameter parameter)
    {
      return new IntroducePrivateGetOnlyAutoPropertyFix(parameter);
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      return cache.GetData(InitializeAutoPropertyFix.InstanceKey) == null && base.IsAvailable(cache);
    }
  }
}