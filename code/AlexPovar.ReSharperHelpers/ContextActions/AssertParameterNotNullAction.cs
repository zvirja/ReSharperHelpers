using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Intentions.CSharp.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Assert parameter is not null (or empty) action", Description = "Assert parameter is not null or empty.")]
  public class AssertParameterNotNullAction : CheckParamNullAction
  {
    private const string ArgumentNotNullAssetionMethod = "ArgumentNotNull";
    private const string ArgumentNotNullOrEmptyAssetionMethod = "ArgumentNotNullOrEmpty";
    private const string AssertTypeName = "Assert";

    public AssertParameterNotNullAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    private bool AssertionAlreadyPresent { get; set; }

    public override string Text => "Assert parameter is not null";

    [NotNull]
    private static IAnchor AssertAnchor { get; } = new InvisibleAnchor(new SubmenuAnchor(IntentionsAnchors.ContextActionsAnchor, SubmenuBehavior.Executable));

    [CanBeNull]
    private IClrTypeName CachedAssertClassTypeName { get; set; }

    protected override IAnchor Anchor => AssertAnchor;

    public override IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToHelpersContextActionIntentions(this.Anchor);
    }

    protected override ICSharpStatement FindParameterCheckAnchor<TParameterDeclaration>(
      IBlock block,
      IParameterDeclaration parameterDeclaration,
      IParameter currentParameter,
      TreeNodeCollection<TParameterDeclaration> parameterDeclarations)
    {
      if (parameterDeclaration == null)
      {
        return null;
      }

      this.AssertionAlreadyPresent = FindAssertionStatement(block, currentParameter) != null;
      if (this.AssertionAlreadyPresent) return null;

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


    [CanBeNull]
    private static ICSharpStatement FindAssertionStatement([NotNull] IBlock block, [CanBeNull] IParameter parameterToMatch)
    {
      return block.StatementsEnumerable
        .OfType<IExpressionStatement>()
        .FirstOrDefault(statement => IsAssertionInvocation(statement, parameterToMatch));
    }

    protected override ICSharpStatement CreateCheckStatement(CSharpElementFactory factory, IParameter parameter, ICSharpExpression parameterName, IPsiModule psiModule)
    {
      if (this.AssertionAlreadyPresent) return null;

      var assertionMethodName = parameter.Type.IsString() ? ArgumentNotNullOrEmptyAssetionMethod : ArgumentNotNullAssetionMethod;

      var assertionMethod = this.FindAssertionMethod(assertionMethodName, parameter.Type, psiModule);

      if (assertionMethod != null)
      {
        return factory.CreateStatement("$0($1, $2);", assertionMethod, parameter, parameterName);
      }

      //Fallback to unresoled
      return factory.CreateStatement("$0.$1($2, $3);", AssertTypeName, assertionMethodName, parameter, parameterName);
    }

    private static bool IsAssertionInvocation([NotNull] IExpressionStatement expressionStatement, [CanBeNull] IParameter parameterToMatch)
    {
      var invocation = expressionStatement.Expression as IInvocationExpression;
      if (invocation == null) return false;

      //Validate that parameter is valid
      if (parameterToMatch != null)
      {
        if (invocation.Arguments.Count != 2) return false;
        var firstArgExpr = invocation.Arguments[0].Expression as IReferenceExpression;
        if (firstArgExpr?.Reference.Resolve().DeclaredElement?.Equals(parameterToMatch) != true) return false;
      }

      //Validate this is Assert.Argument... method.
      var reference = invocation.InvokedExpression as IReferenceExpression;
      if (reference == null) return false;

      var resolveResult = reference.Reference.Resolve();

      //Handle case when reference is not resolved 
      if (resolveResult.ResolveErrorType == ResolveErrorType.NOT_RESOLVED)
      {
        var referenceText = reference.GetText();
        var parts = StringUtil.FullySplitFQName(referenceText);

        if (parts.Length != 2) return false;
        if (parts[0] != AssertTypeName) return false;

        var methodReferenceName = parts[1];

        return methodReferenceName == ArgumentNotNullAssetionMethod || methodReferenceName == ArgumentNotNullOrEmptyAssetionMethod;
      }

      var method = reference.Reference.Resolve().DeclaredElement as IMethod;

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

    [CanBeNull]
    private IMethod FindAssertionMethod([NotNull] string assertionMethodName, [NotNull] IExpressionType parameterType, [NotNull] IPsiModule module)
    {
      var symbolScope = module.GetPsiServices().Symbols.GetSymbolScope(module, true, true);

      IClass typeDecl = null;
      if (this.CachedAssertClassTypeName != null)
      {
        typeDecl = symbolScope.GetTypeElementByCLRName(this.CachedAssertClassTypeName) as IClass;
      }
      else
      {
        var candidates = symbolScope.GetElementsByShortName(AssertTypeName).OfType<IClass>().Where(c => (c.Module as IAssemblyPsiModule)?.Assembly.IsMscorlib != true).ToArray();
        if (candidates.Length == 1) typeDecl = candidates[0];
        if (typeDecl != null) this.CachedAssertClassTypeName = typeDecl.GetClrName();
      }

      return typeDecl?
        .EnumerateMembers(assertionMethodName, true)
        .OfType<IMethod>()
        .FirstOrDefault(m => m.Parameters.Count > 1 && (m.Parameters[0].Type.IsOpenType || parameterType.IsImplicitlyConvertibleTo(m.Parameters[0].Type, module.GetTypeConversionRule())));
    }
  }
}