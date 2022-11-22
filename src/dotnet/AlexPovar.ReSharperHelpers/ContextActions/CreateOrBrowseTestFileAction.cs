using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexPovar.ReSharperHelpers.Helpers;
using AlexPovar.ReSharperHelpers.Settings;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.DocumentManagers.impl;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.DocumentModel;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.Extension;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[ReSharperHelpers] Create or browse test file", Description = "Creates new or opens the existing test file.", Priority = short.MinValue)]
  public class CreateOrBrowseTestFileAction : IBulbAction, IContextAction
  {
    private const string TemplateDescription = "[ReSharperHelpers] TestFile";

    [NotNull] private static readonly string[] TestProjectSuffixes =
      new[]
        {
          "Tests.Unit",
          "Test.Unit",
          "UnitTests",
          "UnitTest",
          "Unit.Tests",
          "Unit.Test",
          "Tests",
          "Test",
        }
        .ToArray();


    [NotNull] private static readonly string[] TestProjectSuffixesWithDelimiters =
      TestProjectSuffixes
        .SelectMany(suffix => new[] { suffix, $".{suffix}" })
        .ToArray();

    [NotNull] private readonly ICSharpContextActionDataProvider _provider;

    public CreateOrBrowseTestFileAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      this._provider = provider;
    }

    [CanBeNull]
    private IProjectFile ExistingProjectFile { get; set; }

    [CanBeNull]
    private IProject CachedTestProject { get; set; }

    public string Text => this.ExistingProjectFile == null ? "Create test file" : "Go to test file";

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToContextActionIntentions(HelperActionsConstants.ContextActionsAnchor, MyIcons.ContextActionIcon);
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      this.ExistingProjectFile = null;
      this.CachedTestProject = null;

      IClassLikeDeclaration typeDeclaration = ClassLikeDeclarationNavigator.GetByNameIdentifier(this._provider.GetSelectedElement<ICSharpIdentifier>());
      if (typeDeclaration is IInterfaceDeclaration) return false;

      ITypeElement declaredType = typeDeclaration?.DeclaredElement;
      if (declaredType == null) return false;

      // Disable for nested classes.
      if (typeDeclaration.GetContainingTypeDeclaration() != null) return false;

      // TRY RESOLVE EXISTING TEST.
      var helperSettings = ReSharperHelperSettings.GetSettings(typeDeclaration.GetSettingsStoreWithEditorConfig());

      var testProject = this.CachedTestProject = this.ResolveTargetTestProject(typeDeclaration, typeDeclaration.GetSolution(), helperSettings);
      if (testProject == null) return false;

      // Skip project if it's the same as current. This way we don't suggest to create tests in test projects.
      if (testProject.Equals(typeDeclaration.GetProject())) return false;

      var testClassRelativeNsParts = GetTestClassRelativeNamespaceParts(typeDeclaration, testProject, helperSettings);
      if (testClassRelativeNsParts == null)
        return false;

      var testClassNs = StringUtil.MakeFQName(testProject.GetDefaultNamespace(), StringUtil.MakeFQName(testClassRelativeNsParts));

      // Resolve candidates for test classes.
      var validTestSuffixes = helperSettings.ValidTestClassNameSuffixes?.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.ToString())
        .Concat(helperSettings.TestClassNameSuffix)
        .Distinct();

      var classTypeFqnCandidates = validTestSuffixes?.Select(suffix => StringUtil.MakeFQName(testClassNs, declaredType.ShortName + suffix));
      if (classTypeFqnCandidates == null) return false;

      var symbolsService = typeDeclaration.GetPsiServices().Symbols;

      var testClass = testProject.GetPsiModules()
        .Select(m => symbolsService.GetSymbolScope(m, false, true))
        .SelectMany(scope => classTypeFqnCandidates.Select(scope.GetTypeElementByCLRName))
        .FirstNotNull();

      this.ExistingProjectFile = testClass?.GetSingleOrDefaultSourceFile()?.ToProjectFile();

      return true;
    }

    public async void Execute(ISolution solution, ITextControl textControl)
    {
      if (this.ExistingProjectFile != null)
      {
        await ShowProjectFile(solution, this.ExistingProjectFile, null);
        return;
      }

      using (ReadLockCookie.Create())
      {
        using (var cookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.Text, NullProgressIndicator.Create()))
        {
          var declaration = this._provider.GetSelectedElement<IClassLikeDeclaration>();

          ITypeElement declaredType = declaration?.DeclaredElement;
          if (declaredType == null)
            return;

          IContextBoundSettingsStore settingsStore = declaration.GetSettingsStoreWithEditorConfig();
          var helperSettings = ReSharperHelperSettings.GetSettings(settingsStore);

          var testProject = this.CachedTestProject ?? this.ResolveTargetTestProject(declaration, solution, helperSettings);
          if (testProject == null)
            return;

          var testClassRelativeNsParts = GetTestClassRelativeNamespaceParts(declaration, testProject, helperSettings);
          if (testClassRelativeNsParts == null)
            return;

          var testFolderLocation = testClassRelativeNsParts.Aggregate(testProject.Location, (current, part) => current.Combine(part));
          var testClassNs = StringUtil.MakeFQName(testProject.GetDefaultNamespace(), StringUtil.MakeFQName(testClassRelativeNsParts));

          var testFolder = testProject.GetOrCreateProjectFolder(testFolderLocation, cookie);
          if (testFolder == null)
            return;

          var testClassName = declaredType.ShortName + helperSettings.TestClassNameSuffix;
          var testFileName = testClassName + ".cs";

          var testFileTemplate = StoredTemplatesProvider.Instance.EnumerateTemplates(settingsStore, TemplateApplicability.File).FirstOrDefault(t => t.Description == TemplateDescription);
          if (testFileTemplate != null)
          {
            await FileTemplatesManager.Instance.CreateFileFromTemplateAsync(testFileName, new ProjectFolderWithLocation(testFolder), testFileTemplate);
            return;
          }

          var newFile = AddNewItemHelper.AddFile(testFolder, testFileName).ToSourceFile();
          if (newFile == null)
            return;

          solution.TryGetComponent<FileHeaderUtils>()?.InsertHeader(solution, newFile, newFile.Document.GetDocumentStartOffset());

          int? caretPosition;
          using (PsiTransactionCookie.CreateAutoCommitCookieWithCachesUpdate(newFile.GetPsiServices(), "CreateTestClass"))
          {
            var csharpFile = (ICSharpFile)newFile.GetDominantPsiFile<CSharpLanguage>().NotNull();

            var elementFactory = CSharpElementFactory.GetInstance(csharpFile);

            bool isFileScoped = CSharpNamespaceUtil.CanAddFileScopedNamespaceDeclaration(csharpFile);
            var namespaceDeclaration = elementFactory.CreateNamespaceDeclaration(testClassNs, isFileScoped);
            var addedNs = csharpFile.AddNamespaceDeclarationAfter(namespaceDeclaration, null);

            var classLikeDeclaration = (IClassLikeDeclaration)elementFactory.CreateTypeMemberDeclaration("public class $0 {}", testClassName);
            var addedTypeDeclaration = addedNs.AddTypeDeclarationAfter(classLikeDeclaration, null) as IClassDeclaration;

            caretPosition = addedTypeDeclaration?.Body?.GetDocumentRange().TextRange.StartOffset + 1;
          }

          cookie.Commit(NullProgressIndicator.Create());

          await ShowProjectFile(solution, newFile.ToProjectFile().NotNull(), caretPosition);
        }
      }
    }

    [CanBeNull, Pure]
    private static string[] GetTestClassRelativeNamespaceParts([NotNull] ITypeDeclaration classDeclaration, [NotNull] IProject testProject, ReSharperHelperSettings helperSettings)
    {
      var currentProject = classDeclaration.GetProject();
      if (currentProject == null) return null;

      var typeElement = classDeclaration.DeclaredElement;
      if (typeElement == null) return null;

      var relativeSourceNsParts = TrimDefaultProjectNamespace(currentProject, typeElement.GetContainingNamespace().QualifiedName);
      if (!helperSettings.TestsProjectSubNamespace.IsNullOrEmpty())
      {
        string[] subDirParts = StringUtil.FullySplitFQName(helperSettings.TestsProjectSubNamespace);
        relativeSourceNsParts = subDirParts.Concat(relativeSourceNsParts).ToArray();
      }

      return relativeSourceNsParts;
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

    [CanBeNull]
    private IProject ResolveTargetTestProject([NotNull] ITreeNode contextNode, [NotNull] ISolution solution, [NotNull] ReSharperHelperSettings helperSettings)
    {
      var explicitProjectName = helperSettings.TestsProjectName;
      if (!string.IsNullOrEmpty(explicitProjectName))
      {
        return solution.GetProjectByName(explicitProjectName);
      }

      // Try to guess project specific test project.
      var currentProjectName = contextNode.GetProject()?.Name;
      if (currentProjectName == null)
      {
        return null;
      }

      var candidate = TestProjectSuffixesWithDelimiters
        .SelectMany(suffix => solution.GetProjectsByName(currentProjectName + suffix))
        .TryGetSingleCandidate();

      if (candidate.isSingle)
      {
        return candidate.value;
      }

      // Optimize, as we are definitely sure that more than one project with test suffix exists.
      if (candidate.hasAny)
      {
        return null;
      }

      // Try to guess global test project.
      candidate = solution.GetAllProjects()
        .Where(static proj => TestProjectSuffixes.Any(suffix => proj.Name.EndsWith(suffix, StringComparison.Ordinal)))
        .TryGetSingleCandidate();

      if (candidate.isSingle)
      {
        return candidate.value;
      }

      return null;
    }

    private static async Task ShowProjectFile([NotNull] ISolution solution, [NotNull] IProjectFile file, int? caretPosition)
    {
      var editor = solution.GetComponent<IEditorManager>();
      var textControl = await editor.OpenProjectFileAsync(file, OpenFileOptions.DefaultActivate);

      if (caretPosition != null)
      {
        ReadLockCookie.GuardedExecute(() => textControl?.Caret.MoveTo(caretPosition.Value, CaretVisualPlacement.DontScrollIfVisible));
      }
    }
  }
}
