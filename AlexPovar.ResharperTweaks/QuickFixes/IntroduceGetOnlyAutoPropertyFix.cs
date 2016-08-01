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
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace AlexPovar.ResharperTweaks.QuickFixes
{
  [QuickFix]
  public class IntroduceGetOnlyAutoPropertyFix : InitializeAutoPropertyFix, IQuickFix
  {
    private string _myPattern;


    public IntroduceGetOnlyAutoPropertyFix(UnusedParameterGlobalWarning error) : base(error)
    {
    }

    public IntroduceGetOnlyAutoPropertyFix(UnusedParameterLocalWarning error) : base(error)
    {
    }

    public IntroduceGetOnlyAutoPropertyFix(IParameter parameter, AccessRights desiredVisibility) : base(parameter)
    {
      this.DesiredVisibility = desiredVisibility;
    }

    private AccessRights DesiredVisibility { get; } = AccessRights.PRIVATE;

    [NotNull]
    public string TextFormat { get; set; } = "Create and initialize {0} get-only auto-property '{1}'";

    public override string Text => string.Format(this.TextFormat, DeclaredElementPresenter.Format(CSharpLanguage.Instance, this.DesiredVisibility), this.MemberName);

    public new IEnumerable<IntentionAction> CreateBulbItems()
    {
      var anchor = MyUtil.CreateGroupAnchor(IntentionsAnchors.QuickFixesAnchor);

      var actions = this.ToQuickFixAction(anchor, MyIcons.YellowBulbIcon);

      actions = actions.Concat(this.CreateAuxiliaryFix(AccessRights.PUBLIC).ToQuickFixAction(anchor, MainThemedIcons.TweaksYellowBulbIcon.Id));

      return actions;
    }

    public IntroduceGetOnlyAutoPropertyFix CreateAuxiliaryFix(AccessRights rights, [CanBeNull] string textFormat = null)
    {
      return new IntroduceGetOnlyAutoPropertyFix(this.myParameter, rights)
      {
        TextFormat = textFormat ?? "Create {0}",
        _myPattern = this._myPattern
      };
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
      Predicate<ITypeMember> anchorMembersFilter = member => (member as IProperty)?.Parameters.Count == 0;
      return new IntroduceAndInitializeReadonlyPropertyExec(this.myParameter, this._myPattern, NamedElementKinds.MethodPropertyEvent, this.DesiredVisibility,
        anchorMembersFilter, this.myLanguageHelper);
    }

    private class IntroduceAndInitializeReadonlyPropertyExec : IMemberFromParameterExec
    {
      private readonly AccessRights _desiredAccessRights;

      private readonly Predicate<ITypeMember> _myAnchorMembersFilter;

      private readonly NamedElementKinds _myKind;

      private readonly IIntroduceFromParameterLanguageHelper _myLanguageHelper;

      private readonly IParameter _myParameter;

      private readonly string _myPattern;

      public IntroduceAndInitializeReadonlyPropertyExec(
        [NotNull] IParameter parameter,
        [NotNull] string pattern,
        NamedElementKinds kind,
        AccessRights desiredAccessRights,
        [NotNull] Predicate<ITypeMember> anchorMembersFilter,
        [NotNull] IIntroduceFromParameterLanguageHelper languageHelper)
      {
        this._myParameter = parameter;
        this._myPattern = pattern;
        this._myKind = kind;
        this._desiredAccessRights = desiredAccessRights;
        this._myAnchorMembersFilter = anchorMembersFilter;
        this._myLanguageHelper = languageHelper;
      }

      public void Execute()
      {
        var member = new IntroduceMemberExec(this._myParameter, this._myPattern, this._myKind, this._myAnchorMembersFilter, this._myLanguageHelper).Execute();

        this.PostProcessProperty(member);

        new InitializeMemberExecAfterAssertions(this._myParameter, member, this._myAnchorMembersFilter, this._myLanguageHelper).Execute();
      }

      private void PostProcessProperty(ITypeMember typeMember)
      {
        var property = typeMember as IProperty;
        var propertyDeclarations = property?.GetDeclarations();
        if (propertyDeclarations == null || propertyDeclarations.Count == 0) return;

        var propertyDecl = (IPropertyDeclaration) propertyDeclarations[0];

        this.MakeGetOnly(propertyDecl);
        this.ChangeAccessRights(propertyDecl);
        this.CopyAnnotationAttributes(propertyDecl);
      }

      private void MakeGetOnly([NotNull] IPropertyDeclaration propertyDeclaration)
      {
        var setterDecl = propertyDeclaration.AccessorDeclarationsEnumerable.FirstOrDefault(accDecl => accDecl.Kind == AccessorKind.SETTER);

        if (setterDecl == null) return;

        propertyDeclaration.RemoveAccessorDeclaration(setterDecl);
      }

      private void ChangeAccessRights([NotNull] IPropertyDeclaration propDecl)
      {
        var accessRights = propDecl.GetAccessRights();
        if (accessRights == this._desiredAccessRights) return;

        propDecl.SetAccessRights(this._desiredAccessRights);
      }

      private void CopyAnnotationAttributes([NotNull] IPropertyDeclaration propDecl)
      {
        var annotations = propDecl.GetPsiServices().GetCodeAnnotationsCache();

        var annotationsAttributes = this._myParameter.GetAttributeInstances(true).Select(attr => attr.GetClrName()).Where(attrType => annotations.IsAnnotationType(attrType, attrType.ShortName));

        foreach (var annotationType in annotationsAttributes)
        {
          var shortName = annotationType.ShortName;
          AnnotationsUtil.CreateAndAddAnnotationAttribute(propDecl, shortName);
        }
      }
    }

    private class InitializeMemberExecAfterAssertions : MemberFromParameterExec
    {
      private readonly ITypeMember myMember;
      private readonly IParameter myParameter;

      public InitializeMemberExecAfterAssertions(
        [NotNull] IParameter parameter,
        [NotNull] ITypeMember member,
        [NotNull] Predicate<ITypeMember> anchorMembersFilter,
        [NotNull] IIntroduceFromParameterLanguageHelper languageHelper)
        : base(parameter, anchorMembersFilter, languageHelper)
      {
        this.myParameter = parameter;
        this.myMember = member;
        this.myAnchorMembersFilter = anchorMembersFilter;
        this.myLanguageHelper = languageHelper;
      }

      public void Execute()
      {
        var assignmentMatch = this.FindStatementPosition();

        IStatement anchorStatement;
        bool insertBefore;

        if (assignmentMatch == null)
        {
          //Anchor was not found. Ensure inserted after assetions.
          anchorStatement = this.FindLastAssertionStatement();

          insertBefore = false;
        }
        else
        {
          anchorStatement = assignmentMatch.AssignmentStatement;
          insertBefore = assignmentMatch.ParameterDeclaration.GetTreeStartOffset() > this.myParameterDeclaration.GetTreeStartOffset();
        }

        this.myLanguageHelper.AddAssignmentToBody(this.myConstructorDeclaration, anchorStatement, insertBefore, this.myParameter, this.myMember);
      }

      [CanBeNull]
      private IStatement FindLastAssertionStatement()
      {
        var ctor = this.myParameter.ContainingParametersOwner as IConstructor;
        var ctorDeclaration = ctor?.GetDeclarations().First() as IFunctionDeclaration;

        if (ctorDeclaration == null) return null;

        foreach (var statement in this.myLanguageHelper.BodyStatements(ctorDeclaration).Reverse())
        {
          var expressionStatement = statement as IExpressionStatement;
          if (expressionStatement != null && AssertParametersNotNullAction.IsAssertStatement(expressionStatement))
          {
            return statement;
          }
        }

        return null;
      }
    }
  }
}