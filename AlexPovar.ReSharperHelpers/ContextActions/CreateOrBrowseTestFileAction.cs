using System;
using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Create or browse test file", Description = "Creates new or opens the existing test file.", Priority = short.MinValue)]
  public class CreateOrBrowseTestFileAction : IBulbAction, IContextAction
  {
    private const string TemplateDescription = "[AlexPovarHelpers] TestFile";

    [NotNull] private readonly ICSharpContextActionDataProvider _myProvider;

    public CreateOrBrowseTestFileAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      if (provider == null) throw new ArgumentNullException(nameof(provider));

      this._myProvider = provider;

      this.IsEnabledForProject = provider.Project?.Name.EndsWith(".Tests") != true;
    }

    private bool IsEnabledForProject { get; }

    public void Execute(ISolution solution, ITextControl textControl)
    {
      using (ReadLockCookie.Create())
      {
        using (CompilationContextCookie.Create(textControl.GetContext(solution)))
        {
          string testFileName;
          IProjectFolder testFolder;
          Template testFileTemplate;

          using (var cookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.Text, NullProgressIndicator.Instance))
          {
            var declaration = this._myProvider.GetSelectedElement<ICSharpTypeDeclaration>();
            var declaredType = declaration?.DeclaredElement;
            if (declaredType == null) return;

            var project = solution.GetProjectsByName("TestProject.Tests").Single();

            var testNamespaceParts = GetExpectedTestNamespacePartsWithoutRoot(project, declaredType.GetContainingNamespace().QualifiedName);
            var testFolderLocation = testNamespaceParts.Aggregate(project.Location, (current, part) => current.Combine(part));

            testFolder = project.GetOrCreateProjectFolder(testFolderLocation, cookie);
            if (testFolder == null) return;

            testFileName = declaredType.ShortName + "Tests.cs";

            testFileTemplate =
              StoredTemplatesProvider.Instance.EnumerateTemplates(declaration.GetSettingsStore(), TemplateApplicability.File).FirstOrDefault(t => t.Description == TemplateDescription);
          }

          if (testFileTemplate != null)
          {
            FileTemplatesManager.Instance.CreateFileFromTemplate(testFileName, testFolder, testFileTemplate);
          }
          else
          {
            var newFile = AddNewItemUtil.AddFile(testFolder, testFileName,
              $"File template with description '{TemplateDescription}' is not defined.{Environment.NewLine}Navigate to Templates Explorer and define appropriate file template.");

            ShowProjectFile(solution, newFile);
          }
        }
      }
    }

    public string Text => "[Helpers] Create test file";

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToContextAction(HelperActionsConstants.ContextActionsAnchor, MyIcons.ContextActionIcon);
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      if (!this.IsEnabledForProject) return false;

      var typeDeclaration = this._myProvider.GetSelectedElement<ICSharpTypeDeclaration>();
      if (typeDeclaration == null) return false;

      //Disable for nested classes
      if (typeDeclaration.GetContainingTypeDeclaration() != null) return false;

      var declaredElement = typeDeclaration.DeclaredElement;
      if (declaredElement == null) return false;

      var typeName = declaredElement.ShortName;
      if (typeName.EndsWith("Test") || typeName.EndsWith("Tests")) return false;

      return true;
    }

    private static string[] GetExpectedTestNamespacePartsWithoutRoot([NotNull] IProject project, [NotNull] string classNamespace)
    {
      var namespaceParts = StringUtil.FullySplitFQName(classNamespace);

      var defaultNamespace = project.GetDefaultNamespace();
      if (defaultNamespace != null)
      {
        var parts = StringUtil.FullySplitFQName(defaultNamespace);

        namespaceParts = namespaceParts.SkipWhile((part, index) => part.Equals(parts[index], StringComparison.Ordinal)).ToArray();
      }

      return namespaceParts;
    }

    private static void ShowProjectFile([NotNull] ISolution solution, [NotNull] IProjectFile file)
    {
      var editor = solution.GetComponent<IEditorManager>();
      editor.OpenProjectFile(file, true);
    }
  }
}