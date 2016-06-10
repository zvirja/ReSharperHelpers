using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.UI;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Copy full class name", Description = "Copy full class name.", Priority = short.MinValue)]
  public class CopyFullClassNameAction : TweaksContextActionBase
  {
    [NotNull] private readonly Clipboard _clipboard;
    [NotNull] private readonly ICSharpContextActionDataProvider _provider;

    public CopyFullClassNameAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      if (provider == null) throw new ArgumentNullException(nameof(provider));

      _provider = provider;
      _clipboard = Shell.Instance.GetComponent<Clipboard>().NotNull("Unable to resolve clipboard service.");
    }

    public override string Text => "[Tweaks] Copy full class name";

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var classDeclaration = _provider.GetSelectedElement<ICSharpIdentifier>()?.Parent as IClassDeclaration;
      var declaredClass = classDeclaration?.DeclaredElement;

      if (declaredClass == null) return null;

      var typeName = declaredClass.GetClrName().FullName;
      var moduleName = declaredClass.Module.Name;

      var fullName = $"{typeName}, {moduleName}";
      _clipboard.SetText(fullName);

      return null;
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      var classDeclaration = _provider.GetSelectedElement<ICSharpIdentifier>()?.Parent as IClassDeclaration;
      return classDeclaration != null;
    }
  }
}