using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.UI;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Copy full class name", Description = "Copy full class name.", Priority = short.MinValue)]
  public class CopyFullClassNameAction : HelpersContextActionBase
  {
    [NotNull] private readonly Clipboard _clipboard;
    [NotNull] private readonly ICSharpContextActionDataProvider _provider;
    [CanBeNull] private IClassLikeDeclaration Declaration { get; set; }

    public CopyFullClassNameAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      if (provider == null) throw new ArgumentNullException(nameof(provider));

      this._provider = provider;
      this._clipboard = Shell.Instance.GetComponent<Clipboard>().NotNull("Unable to resolve clipboard service.");
    }

    public override string Text
    {
      get
      {
        var type = "";
        var declaredElement = this.Declaration?.DeclaredElement;
        if (declaredElement != null)
        {
          type = DeclaredElementPresenter.Format(this.Declaration.Language, DeclaredElementPresenter.KIND_PRESENTER, declaredElement);
        }

        return $"[Helpers] Copy full {type} name";
      }
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var declaredElement = this.Declaration?.DeclaredElement;
      if (declaredElement == null) return null;

      var typeName = declaredElement.GetClrName().FullName;
      var moduleName = declaredElement.Module.Name;

      var fullName = $"{typeName}, {moduleName}";
      this._clipboard.SetText(fullName);

      return null;
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      this.Declaration = ClassLikeDeclarationNavigator.GetByNameIdentifier(this._provider.GetSelectedElement<ICSharpIdentifier>());
      return this.Declaration != null;
    }
  }
}