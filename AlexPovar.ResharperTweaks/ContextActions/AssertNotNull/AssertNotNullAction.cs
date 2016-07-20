using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Intentions.CSharp.ContextActions;
using JetBrains.ReSharper.Intentions.CSharp.ContextActions.CheckParameters;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions.AssertNotNull
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Assert not null (or empty) action", Description = "Assert parameter for null or empty.", Priority = short.MaxValue)]
  public class AssertNotNullAction : ParameterCheckActionBase
  {
    private const string ArgumentNotNullAssetionMethod = "ArgumentNotNull";
    private const string ArgumentNotNullOrEmptyAssetionMethod = "ArgumentNotNullOrEmpty";
    private const string AssertTypeName = "Assert";

    private static readonly Func<IDeclaration, bool> NeedsAnnotationInvoker =
      MyReflectionUtil.CreateStaticMethodInvocationDelegate<Func<IDeclaration, bool>>(typeof (CheckParamNullAction), "NeedsAnnotation");

    private static readonly Action<IDeclaration> AddAnnotationInvoker =
      MyReflectionUtil.CreateStaticMethodInvocationDelegate<Action<IDeclaration>>(typeof (CheckParamNullAction), "AddAnnotation");

    public AssertNotNullAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
      this.AssertAllAction = new AssertAllParamsNotNullAction(provider, this);
    }

    protected AssertNotNullAction(ICSharpContextActionDataProvider provider, bool recursionGuard) : base(provider)
    {
    }

    private AssertAllParamsNotNullAction AssertAllAction { get; }

    private bool AssertAllIsAvailable { get; set; }

    public override string Text => "Assert parameter is not null";

    public override IEnumerable<IntentionAction> CreateBulbItems()
    {
      if (!this.AssertAllIsAvailable) return this.ToContextAction(null, MyIcons.ContextActionIcon);

      var anchor = new ExecutableGroupAnchor(IntentionsAnchors.ContextActionsAnchor, duplicateFirstItem: false);

      var actions = new[]
      {
        this.ToContextAction(anchor, MyIcons.ContextActionIcon),
        this.AssertAllAction.ToContextAction(anchor, MyIcons.ContextActionIcon)
      };

      return actions.SelectMany(x => x);
    }

    protected override ICSharpStatement FindParameterCheckAnchor<TParameterDeclaration>(IBlock block, IParameterDeclaration parameterDeclaration,
      TreeNodeCollection<TParameterDeclaration> parameterDeclarations)
    {
      if (parameterDeclaration == null)
      {
        return null;
      }

      //We take last parameter assertion before our parameter. If nothing - create at the beginning.
      foreach (var current in parameterDeclarations.TakeWhile(x => !parameterDeclaration.Equals(x)).Reverse())
      {
        var parameter = current.DeclaredElement;
        if (parameter == null) continue;

        var assertionStatement = FindAssertionStatement(block, parameter);
        if (assertionStatement != null) return assertionStatement;
      }

      return null;
    }


    public override bool IsAvailable(IUserDataHolder cache)
    {
      this.AssertAllIsAvailable = this.AssertAllAction.IsAvailable(cache);

      return base.IsAvailable(cache);
    }

    protected override ITypeElement GetExceptionType(IPsiModule psiModule)
    {
      var predefinedType = psiModule.GetPredefinedType();
      return predefinedType.ArgumentNullException.GetTypeElement();
    }

    public static bool IsAssertStatement([NotNull] IExpressionStatement statement)
    {
      return IsAssertionInvocation(statement, null);
    }

    [CanBeNull]
    private static ICSharpStatement FindAssertionStatement([NotNull] IBlock block, [CanBeNull] IParameter parameterToMatch)
    {
      return block.StatementsEnumerable
        .OfType<IExpressionStatement>()
        .FirstOrDefault(statement => IsAssertionInvocation(statement, parameterToMatch));
    }

    private static bool IsAssertionInvocation([NotNull] IExpressionStatement expressionStatement, [CanBeNull] IParameter parameterToMatch)
    {
      var invocation = expressionStatement.Expression as IInvocationExpression;
      if (invocation == null) return false;

      //Validate that parameter is valid
      if (parameterToMatch != null)
      {
        if (invocation?.Arguments.Count != 2) return false;
        var firstArgExpr = invocation.Arguments[0].Expression as IReferenceExpression;
        if (firstArgExpr?.Reference.Resolve().DeclaredElement?.Equals(parameterToMatch) != true) return false;
      }

      //Validate this is Assert.Argument... method.
      var reference = invocation.InvokedExpression as IReferenceExpression;

      var method = reference?.Reference.Resolve().DeclaredElement as IMethod;

      if (method == null) return false;

      var methodName = method.ShortName;
      if (methodName != ArgumentNotNullAssetionMethod && methodName != ArgumentNotNullOrEmptyAssetionMethod)
      {
        return false;
      }

      var type = method.GetContainingType();
      var typeName = type?.ShortName;

      return typeName == AssertTypeName;
    }

    protected override bool IsAvailableForParameterType(IType type, ITypeElement context, IPsiModule psiModule)
    {
      return CheckParamNullAction.IsNullableType(type);
    }

    protected override ICSharpStatement AddCheckToBlock<TParameterDeclaration>(IBlock block, IParameterDeclaration parameterDeclaration, IParameter parameter,
      TreeNodeCollection<TParameterDeclaration> declarations)
    {
      AddAnnotationIfNeeded(parameterDeclaration);

      var alreadyPresentAssertion = FindAssertionStatement(block, parameter);
      if (alreadyPresentAssertion != null) return alreadyPresentAssertion;

      return base.AddCheckToBlock(block, parameterDeclaration, parameter, declarations);
    }

    protected override ICSharpStatement CreateCheckStatement(CSharpElementFactory factory, IParameter parameter, ICSharpExpression parameterName, IPsiModule psiModule)
    {
      var assertionMethodName = parameter.Type.IsString() ? ArgumentNotNullOrEmptyAssetionMethod : ArgumentNotNullAssetionMethod;

      return factory.CreateStatement("$0.$1($2, $3);", AssertTypeName, assertionMethodName, parameter, parameterName);
    }

    private static void AddAnnotationIfNeeded([CanBeNull] IDeclaration declaration)
    {
      if (NeedsAnnotationInvoker.Invoke(declaration))
      {
        AddAnnotationInvoker.Invoke(declaration);
      }
    }

    private class AssertAllParamsNotNullAction : ContextActionBase
    {
      private static readonly ExecuteOverParameterDelegate ExecuteOverParameterInvoker =
        MyReflectionUtil.CreateInstanceMethodInvocationDelegate<ExecuteOverParameterDelegate>(typeof (ParameterCheckActionBase), "ExecuteOverParameter");

      private readonly ICSharpContextActionDataProvider _myProvider;

      private IList<ICSharpParameterDeclaration> _myParameterDeclarations = EmptyArray<ICSharpParameterDeclaration>.Instance;

      public AssertAllParamsNotNullAction(ICSharpContextActionDataProvider myProvider, AssertNotNullAction assertNotNullAction)
      {
        this._myProvider = myProvider;
        this.AssertNotNullAction = assertNotNullAction;
      }

      private AssertNotNullAction AssertNotNullAction { get; }

      public override string Text => "Assert all parameters are not null";

      protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
      {
        var list = new List<ICSharpStatement>();
        foreach (var current in this._myParameterDeclarations)
        {
          list.AddRange(ExecuteOverParameterInvoker.Invoke(this.AssertNotNullAction, current));
        }

        return this.AssertNotNullAction.HandleAddedStatements(list.ToArray());
      }

      public override bool IsAvailable(IUserDataHolder cache)
      {
        var selectedParameterDeclaration = this._myProvider.GetSelectedElement<ICSharpParameterDeclaration>();
        if (selectedParameterDeclaration == null) return false;

        //TODO: Use GetMultipleParameterDeclarations() method from base as soon as it's available
        var multipleParameterDeclarations = GetMultipleParameterDeclarations(selectedParameterDeclaration);
        if (multipleParameterDeclarations == null) return false;

        var localList = default(LocalList<ICSharpParameterDeclaration>);
        foreach (var current in multipleParameterDeclarations)
        {
          if (current == null) return false;

          if (this.IsAvailableWithParameter(current))
          {
            localList.Add(current);
          }
        }

        if (localList.Count < 2)
        {
          return false;
        }

        this._myParameterDeclarations = localList.ResultingList();
        return true;
      }

      private bool IsAvailableWithParameter([NotNull] ICSharpParameterDeclaration parameterDeclaration)
      {
        //TODO: Use base method directly as soon as it's available.
        if (!parameterDeclaration.IsValid())
        {
          return false;
        }

        var declaredElement = parameterDeclaration.DeclaredElement;
        if (declaredElement == null)
        {
          return false;
        }

        if (declaredElement.Kind == ParameterKind.OUTPUT)
        {
          return false;
        }

        if (declaredElement.ShortName == "???")
        {
          return false;
        }

        var psiModule = this._myProvider.PsiModule;
        var type = declaredElement.Type;
        if (!this.AssertNotNullAction.IsAvailableForParameterType(type, declaredElement.GetContainingType(), psiModule))
        {
          return false;
        }

        return true;
      }

      [CanBeNull]
      private static IEnumerable<ICSharpParameterDeclaration> GetMultipleParameterDeclarations(ICSharpParameterDeclaration parameterDeclaration)
      {
        var ownerDeclaration = CSharpParametersOwnerDeclarationNavigator.GetByParameterDeclaration(parameterDeclaration);
        if (ownerDeclaration == null) return null;

        if (ownerDeclaration.ParameterDeclarations.Count < 2) return null;

        return ownerDeclaration.ParameterDeclarationsEnumerable;
      }

      private delegate IEnumerable<ICSharpStatement> ExecuteOverParameterDelegate(ParameterCheckActionBase instance, [NotNull] ICSharpParameterDeclaration declaration);
    }
  }
}