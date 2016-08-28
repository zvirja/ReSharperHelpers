using System;
using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using AlexPovar.ReSharperHelpers.Settings;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
using JetBrains.DocumentManagers.impl;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Modules;
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
    private const string TemplateDescription = "[AlexHelpers] TestFile";

    [NotNull] private static readonly string[] TestProjectSuffixes =
      new[]
        {
          "Test",
          "Tests",
          "UnitTest",
          "UnitTests"
        }
        .SelectMany(suffix => new[] {"", "."}.Select(delimiter => delimiter + suffix))
        .ToArray();

    [NotNull] private readonly ICSharpContextActionDataProvider _provider;

    public CreateOrBrowseTestFileAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      this._provider = provider;

      this.IsEnabledForProject = ResolveIsEnabledForProject(provider.Project);
    }

    private bool IsEnabledForProject { get; }

    [CanBeNull]
    private IProjectFile ExistingProjectFile { get; set; }

    public void Execute(ISolution solution, ITextControl textControl)
    {
      if (this.ExistingProjectFile != null)
      {
        ShowProjectFile(solution, this.ExistingProjectFile, null);
        return;
      }

      using (ReadLockCookie.Create())
      {
        string testClassName;
        string testNamespace;
        string testFileName;
        IProjectFolder testFolder;
        Template testFileTemplate;

        using (var cookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.Text, NullProgressIndicator.Instance))
        {
          var declaration = this._provider.GetSelectedElement<ICSharpTypeDeclaration>();
          var declaredType = declaration?.DeclaredElement;
          if (declaredType == null) return;


          var settingsStore = declaration.GetSettingsStore();
          var testProject = this.ResolveTargetTestProject(declaration, solution, settingsStore);
          if (testProject == null) return;

          var originalNamespaceParts = TrimDefaultProjectNamespace(declaration.GetProject(), declaredType.GetContainingNamespace().QualifiedName);
          var testFolderLocation = originalNamespaceParts.Aggregate(testProject.Location, (current, part) => current.Combine(part));

          testNamespace = StringUtil.MakeFQName(testProject.GetDefaultNamespace(), StringUtil.MakeFQName(originalNamespaceParts));

          testFolder = testProject.GetOrCreateProjectFolder(testFolderLocation, cookie);
          if (testFolder == null) return;

          testClassName = MakeTestClassName(declaredType.ShortName);
          testFileName = testClassName + ".cs";

          testFileTemplate = StoredTemplatesProvider.Instance.EnumerateTemplates(settingsStore, TemplateApplicability.File).FirstOrDefault(t => t.Description == TemplateDescription);

          cookie.Commit(NullProgressIndicator.Instance);
        }

        if (testFileTemplate != null)
        {
          FileTemplatesManager.Instance.CreateFileFromTemplate(testFileName, new ProjectFolderWithLocation(testFolder), testFileTemplate);
        }
        else
        {
          var newFile = AddNewItemUtil.AddFile(testFolder, testFileName);
          int? caretPosition = -1;

          solution.GetPsiServices().Transactions.Execute(this.Text, () =>
          {
            var psiSourceFile = newFile.ToSourceFile();

            var csharpFile = psiSourceFile?.GetDominantPsiFile<CSharpLanguage>() as ICSharpFile;
            if (csharpFile == null) return;

            var elementFactory = CSharpElementFactory.GetInstance(csharpFile);

            var namespaceDeclaration = elementFactory.CreateNamespaceDeclaration(testNamespace);
            var addedNs = csharpFile.AddNamespaceDeclarationAfter(namespaceDeclaration, null);

            var classLikeDeclaration = (IClassLikeDeclaration) elementFactory.CreateTypeMemberDeclaration("public class $0 {}", testClassName);
            var addedTypeDeclaration = addedNs.AddTypeDeclarationAfter(classLikeDeclaration, null) as IClassDeclaration;

            caretPosition = addedTypeDeclaration?.Body?.GetDocumentRange().TextRange.StartOffset + 1;
          });

          ShowProjectFile(solution, newFile, caretPosition);
        }
      }
    }

    public string Text => this.ExistingProjectFile == null ? "[Helpers] Create test file" : "[Helpers] Go to test file";

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToContextActionIntentions(HelperActionsConstants.ContextActionsAnchor, MyIcons.ContextActionIcon);
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      this.ExistingProjectFile = null;
      if (!this.IsEnabledForProject) return false;

      var classDeclaration = ClassDeclarationNavigator.GetByNameIdentifier(this._provider.GetSelectedElement<ICSharpIdentifier>());
      if (classDeclaration == null) return false;

      //Disable for nested classes
      if (classDeclaration.GetContainingTypeDeclaration() != null) return false;

      var declaredElement = classDeclaration.DeclaredElement;
      if (declaredElement == null) return false;

      //TRY RESOLVE EXISTING TEST
      var symbolScope = classDeclaration.GetPsiServices().Symbols.GetSymbolScope(LibrarySymbolScope.NONE, true);

      var typeName = declaredElement.ShortName;
      var alreadyDeclaredClasses = symbolScope.GetElementsByShortName(MakeTestClassName(typeName)).OfType<IClass>().Where(c => c != null).ToArray();
      if (alreadyDeclaredClasses.Length == 0) return true;

      var myProject = classDeclaration.GetProject();
      if (myProject == null) return true;

      var expectedNamespaceParts = TrimDefaultProjectNamespace(myProject, declaredElement.GetContainingNamespace().QualifiedName);

      var exactMatchTestClass = alreadyDeclaredClasses
        .Where(
          testCandidateClass =>
          {
            var testProj = (testCandidateClass.Module as IProjectPsiModule)?.Project;
            if (testProj == null) return false;

            var actualNamespaceParts = TrimDefaultProjectNamespace(testProj, testCandidateClass.GetContainingNamespace().QualifiedName);
            if (actualNamespaceParts.Length != expectedNamespaceParts.Length) return false;

            return expectedNamespaceParts.SequenceEqual(actualNamespaceParts, StringComparer.Ordinal);
          })
        .FirstOrDefault();

      this.ExistingProjectFile = exactMatchTestClass?.GetSingleOrDefaultSourceFile()?.ToProjectFile();

      return true;
    }

    [CanBeNull]
    private IProject ResolveTargetTestProject([NotNull] ITreeNode contextNode, [NotNull] ISolution solution, [NotNull] IContextBoundSettingsStore settingsStore)
    {
      var helperSettings = settingsStore.GetKey<ReSharperHelperSettings>(SettingsOptimization.OptimizeDefault);

      //Check whether we have configured value.
      var projectName = helperSettings.TestsProjectName;
      if (!string.IsNullOrEmpty(projectName))
      {
        var project = solution.GetProjectByName(projectName);
        if (project == null)
        {
          MessageBox.ShowError($"Unable to find '{projectName}' project that is configured as tests project. Ensure project name is specified correctly in settings.", "ReSharper Helpers");
          return null;
        }
      }

      //Try to guess project name by suffix
      var currentProjectName = contextNode.GetProject()?.Name;
      if (currentProjectName != null)
      {
        var candidates = TestProjectSuffixes
          .SelectMany(suffix => solution.GetProjectsByName(currentProjectName + suffix))
          .WhereNotNull()
          .ToArray();

        if (candidates.Length == 1) return candidates[0];
      }

      MessageBox.ShowError($"Unable to resolve test file project for current class.{Environment.NewLine}Please specify project name in configuration.", "ReSharper Helpers");
      return null;
    }

    [NotNull, Pure]
    private static string MakeTestClassName([NotNull] string className) => className + "Tests";

    private static bool ResolveIsEnabledForProject([CanBeNull] IProject project)
    {
      if (project == null) return false;

      var contextRange = ContextRange.Smart(project.ToDataContext());

      var settingsStore = Shell.Instance.GetComponent<SettingsStore>();

      var settingsStoreBound = settingsStore.BindToContextTransient(contextRange);
      var mySettings = settingsStoreBound.GetKey<ReSharperHelperSettings>(SettingsOptimization.OptimizeDefault);

      return !project.Name.Equals(mySettings.TestsProjectName);
    }


    [NotNull, Pure]
    private static string[] TrimDefaultProjectNamespace([NotNull] IProject project, [NotNull] string classNamespace)
    {
      var namespaceParts = StringUtil.FullySplitFQName(classNamespace);

      var defaultNamespace = project.GetDefaultNamespace();
      if (defaultNamespace != null)
      {
        var parts = StringUtil.FullySplitFQName(defaultNamespace);

        namespaceParts = namespaceParts.SkipWhile((part, index) => index < parts.Length && part.Equals(parts[index], StringComparison.Ordinal)).ToArray();
      }

      return namespaceParts;
    }

    private static void ShowProjectFile([NotNull] ISolution solution, [NotNull] IProjectFile file, int? caretPosition)
    {
      var editor = solution.GetComponent<IEditorManager>();
      var textControl = editor.OpenProjectFile(file, true);

      if (caretPosition != null) textControl?.Caret.MoveTo(caretPosition.Value, CaretVisualPlacement.DontScrollIfVisible);
    }
  }
}