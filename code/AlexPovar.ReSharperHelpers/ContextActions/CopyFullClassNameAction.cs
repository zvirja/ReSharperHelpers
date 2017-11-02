using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.Components;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[ReSharperHelpers] Copy full class name", Description = "Copy full class name.", Priority = short.MinValue)]
  public class CopyFullClassNameAction : HelpersContextActionBase
  {
    [NotNull] private readonly Clipboard _clipboard;
    [NotNull] private readonly ICSharpContextActionDataProvider _provider;

    public CopyFullClassNameAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      this._provider = provider ?? throw new ArgumentNullException(nameof(provider));
      this._clipboard = Shell.Instance.GetComponent<Clipboard>().NotNull("Unable to resolve clipboard service.");
    }

    [CanBeNull]
    private IClassLikeDeclaration Declaration { get; set; }

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
      var moduleName = declaredElement.Module.DisplayName;

      if (declaredElement.Module is IProjectPsiModule projectModule)
      {
        var proj = projectModule.Project;
        moduleName = proj.GetOutputAssemblyName(proj.GetCurrentTargetFrameworkId());
      }

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