using System;
using System.Collections.Generic;
using System.Linq;
using AlexPovar.ResharperTweaks.ContextActions;
using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Intentions.QuickFixes.UsageChecking;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Util;

namespace AlexPovar.ResharperTweaks.QuickFixes
{
  [QuickFix]
  public class IntroducePrivateAutoPropertyFix : InitializeAutoPropertyFix, IQuickFix
  {
    private string _myPattern;


    public IntroducePrivateAutoPropertyFix(UnusedParameterGlobalWarning error) : base(error)
    {
    }

    public IntroducePrivateAutoPropertyFix(UnusedParameterLocalWarning error) : base(error)
    {
    }

    public IntroducePrivateAutoPropertyFix(IParameter parameter) : base(parameter)
    {
    }

    public override string Text => $"Create and initialize private readonly auto-property '{this.MemberName}'";

    public new IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToQuickFixAction(customIconId: MainThemedIcons.TweaksYellowBulbIcon.Id);
    }

    protected override bool IsAvailableEx(ITypeElement typeElement, string memberName)
    {
      if (typeElement.HasMembers(memberName, true) || this.myParameter.GetDeclarations().Count == 0)
      {
        return false;
      }
      var declaration = this.myParameter.GetDeclarations()[0];
      if (declaration == null) return false;

      if (!declaration.IsCSharp6Supported()) return false;

      this._myPattern = this.myLanguageHelper.IntroduceAutoPropertyPattern(declaration);

      return this._myPattern != null;
    }

    public override IMemberFromParameterExec CreateExec()
    {
      Predicate<ITypeMember> anchorMembersFilter = delegate(ITypeMember member)
      {
        var property = member as IProperty;
        return property != null && property.Parameters.Count == 0;
      };
      return new IntroduceAndInitializeReadonlyPropertyExec(this.myParameter, this._myPattern,
        NamedElementKinds.MethodPropertyEvent, anchorMembersFilter, this.myLanguageHelper);
    }

    private class IntroduceAndInitializeReadonlyPropertyExec : IMemberFromParameterExec
    {
      private readonly Predicate<ITypeMember> _myAnchorMembersFilter;

      private readonly NamedElementKinds _myKind;

      private readonly IIntroduceFromParameterLanguageHelper _myLanguageHelper;

      private readonly IParameter _myParameter;

      private readonly string _myPattern;

      public IntroduceAndInitializeReadonlyPropertyExec(
        [NotNull] IParameter parameter,
        [NotNull] string pattern,
        NamedElementKinds kind,
        [NotNull] Predicate<ITypeMember> anchorMembersFilter,
        [NotNull] IIntroduceFromParameterLanguageHelper languageHelper)
      {
        this._myParameter = parameter;
        this._myPattern = pattern;
        this._myKind = kind;
        this._myAnchorMembersFilter = anchorMembersFilter;
        this._myLanguageHelper = languageHelper;
      }

      public void Execute()
      {
        var member =
          new IntroduceMemberExec(this._myParameter, this._myPattern, this._myKind, this._myAnchorMembersFilter,
            this._myLanguageHelper)
            .Execute();

        this.PostProcessProperty(member);

        new InitializeMemberExec(this._myParameter, member, this._myAnchorMembersFilter, this._myLanguageHelper)
          .Execute();
      }

      private void PostProcessProperty(ITypeMember typeMember)
      {
        var property = typeMember as IProperty;
        var propertyDeclarations = property?.GetDeclarations();
        if (propertyDeclarations == null || propertyDeclarations.Count == 0) return;

        var propertyDecl = (IPropertyDeclaration) propertyDeclarations[0];

        this.MakeGetOnly(propertyDecl);
        this.MakePrivate(propertyDecl);
        this.CopyAnnotationAttributes(propertyDecl);
      }

      private void MakeGetOnly([NotNull] IPropertyDeclaration propertyDeclaration)
      {
        var setterDecl =
          propertyDeclaration.AccessorDeclarationsEnumerable.FirstOrDefault(
            accDecl => accDecl.Kind == AccessorKind.SETTER);

        if (setterDecl == null) return;

        propertyDeclaration.RemoveAccessorDeclaration(setterDecl);
      }

      private void MakePrivate([NotNull] IPropertyDeclaration propDecl)
      {
        var accessRights = propDecl.GetAccessRights();
        if (accessRights == AccessRights.PRIVATE) return;

        propDecl.SetAccessRights(AccessRights.PRIVATE);
      }

      private void CopyAnnotationAttributes([NotNull] IPropertyDeclaration propDecl)
      {
        var annotations = propDecl.GetPsiServices().GetCodeAnnotationsCache();

        var annotationsAttributes =
          this._myParameter.GetAttributeInstances(true)
            .Select(attr => attr.GetClrName())
            .Where(attrType => annotations.IsAnnotationType(attrType, attrType.ShortName));

        foreach (var annotationType in annotationsAttributes)
        {
          var shortName = annotationType.ShortName;
          AnnotationsUtil.CreateAndAddAnnotationAttribute(propDecl, shortName);
        }
      }
    }
  }
}