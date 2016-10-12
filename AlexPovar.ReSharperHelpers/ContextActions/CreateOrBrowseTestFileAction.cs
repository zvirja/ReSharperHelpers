using System;
using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using AlexPovar.ReSharperHelpers.Settings;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentManagers.impl;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.IDE;
using JetBrains.Metadata.Reader.Impl;
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
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.Extension;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[AlexHelpers] Create or browse test file", Description = "Creates new or opens the existing test file.", Priority = short.MinValue)]
  public class CreateOrBrowseTestFileAction : IBulbAction, IContextAction
  {
    private const string TemplateDescription = "[AlexHelpers] TestFile";

    [NotNull] private static readonly ClrTypeName AssemblyMetadataAttributeName = new ClrTypeName("System.Reflection.AssemblyMetadataAttribute");

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
    }

    [CanBeNull]
    private IProjectFile ExistingProjectFile { get; set; }

    [CanBeNull]
    private IProject CachedTestProject { get; set; }

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
          var helperSettings = ReSharperHelperSettings.GetSettings(settingsStore);

          var testProject = this.CachedTestProject ?? this.ResolveTargetTestProject(declaration, solution, helperSettings);
          if (testProject == null) return;

          var classNamespaceParts = TrimDefaultProjectNamespace(declaration.GetProject().NotNull(), declaredType.GetContainingNamespace().QualifiedName);
          var testFolderLocation = classNamespaceParts.Aggregate(testProject.Location, (current, part) => current.Combine(part));

          testNamespace = StringUtil.MakeFQName(testProject.GetDefaultNamespace(), StringUtil.MakeFQName(classNamespaceParts));

          testFolder = testProject.GetOrCreateProjectFolder(testFolderLocation, cookie);
          if (testFolder == null) return;

          testClassName = declaredType.ShortName + helperSettings.TestClassNameSuffix;
          testFileName = testClassName + ".cs";

          testFileTemplate = StoredTemplatesProvider.Instance.EnumerateTemplates(settingsStore, TemplateApplicability.File).FirstOrDefault(t => t.Description == TemplateDescription);

          cookie.Commit(NullProgressIndicator.Instance);
        }

        if (testFileTemplate != null)
        {
          FileTemplatesManager.Instance.CreateFileFromTemplate(testFileName, new ProjectFolderWithLocation(testFolder), testFileTemplate);
          return;
        }

        var newFile = AddNewItemUtil.AddFile(testFolder, testFileName);
        if (newFile == null) return;

        int? caretPosition = -1;
        solution.GetPsiServices().Transactions.Execute(this.Text, () =>
        {
          var psiSourceFile = newFile.ToSourceFile();

          var csharpFile = psiSourceFile?.GetDominantPsiFile<CSharpLanguage>() as ICSharpFile;
          if (csharpFile == null) return;

          var elementFactory = CSharpElementFactory.GetInstance(csharpFile);

          var namespaceDeclaration = elementFactory.CreateNamespaceDeclaration(testNamespace);
          var addedNs = csharpFile.AddNamespaceDeclarationAfter(namespaceDeclaration, null);

          var classLikeDeclaration = (IClassLikeDeclaration)elementFactory.CreateTypeMemberDeclaration("public class $0 {}", testClassName);
          var addedTypeDeclaration = addedNs.AddTypeDeclarationAfter(classLikeDeclaration, null) as IClassDeclaration;

          caretPosition = addedTypeDeclaration?.Body?.GetDocumentRange().TextRange.StartOffset + 1;
        });

        ShowProjectFile(solution, newFile, caretPosition);
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
      this.CachedTestProject = null;

      var classDeclaration = ClassDeclarationNavigator.GetByNameIdentifier(this._provider.GetSelectedElement<ICSharpIdentifier>());
      var declaredType = classDeclaration?.DeclaredElement;
      if (declaredType == null) return false;

      //Disable for nested classes
      if (classDeclaration.GetContainingTypeDeclaration() != null) return false;

      //TRY RESOLVE EXISTING TEST
      var helperSettings = ReSharperHelperSettings.GetSettings(classDeclaration.GetSettingsStore());

      var testProject = this.CachedTestProject = this.ResolveTargetTestProject(classDeclaration, classDeclaration.GetSolution(), helperSettings);
      if (testProject == null) return false;

      //Skip project if it's the same as current. This way we don't suggest to create tests in test projects.
      if (testProject.Equals(classDeclaration.GetProject())) return false;

      var testClassNamespaceParts = MakeTestClassNamespaceParts(classDeclaration, testProject);
      if (testClassNamespaceParts == null) return false;
      var testNamespace = StringUtil.MakeFQName(testClassNamespaceParts);

      //Resolve candidates for test classes
      var validTestSuffixes = helperSettings.ValidTestClassNameSuffixes?.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.ToString())
        .Concat(helperSettings.TestClassNameSuffix)
        .Distinct();

      var classTypeFqnCandidates = validTestSuffixes?.Select(suffix => StringUtil.MakeFQName(testNamespace, declaredType.ShortName + suffix));
      if (classTypeFqnCandidates == null) return false;

      var symbolsService = classDeclaration.GetPsiServices().Symbols;

      var testClass = testProject.GetPsiModules()
        .Select(m => symbolsService.GetSymbolScope(m, false, true))
        .SelectMany(scope => classTypeFqnCandidates.Select(scope.GetTypeElementByCLRName))
        .FirstNotNull();

      this.ExistingProjectFile = testClass?.GetSingleOrDefaultSourceFile()?.ToProjectFile();

      return true;
    }

    [CanBeNull, Pure]
    private static string[] MakeTestClassNamespaceParts([NotNull] IClassDeclaration classDeclaration, [NotNull] IProject testProject)
    {
      var currentProject = classDeclaration.GetProject();
      if (currentProject == null) return null;

      var typeElement = classDeclaration.DeclaredElement;
      if (typeElement == null) return null;

      var relativeTypeNamespace = TrimDefaultProjectNamespace(currentProject, typeElement.GetContainingNamespace().QualifiedName);

      var splittedDefaultNamespace = StringUtil.FullySplitFQName(testProject.GetDefaultNamespace());
      return splittedDefaultNamespace.Concat(relativeTypeNamespace).ToArray();
    }

    [CanBeNull]
    private IProject ResolveTargetTestProject([NotNull] ITreeNode contextNode, [NotNull] ISolution solution, [NotNull] ReSharperHelperSettings helperSettings)
    {
      //Get project by assembly attribute (if present)
      var projectName = solution.GetPsiServices().Symbols
        .GetModuleAttributes(contextNode.GetPsiModule())
        .GetAttributeInstances(AssemblyMetadataAttributeName, false)
        .Select(TryExtractProjectNameFromAssemblyMetadataAttribute)
        .FirstOrDefault(n => n != null);

      //Check whether we have configured global test project.
      if (string.IsNullOrEmpty(projectName))
      {
        projectName = helperSettings.TestsProjectName;
      }

      if (!string.IsNullOrEmpty(projectName))
      {
        return solution.GetProjectByName(projectName);
      }

      //Try to guess project specific test project.
      var currentProjectName = contextNode.GetProject()?.Name;
      if (currentProjectName == null) return null;

      var candidates = TestProjectSuffixes
        .SelectMany(suffix => solution.GetProjectsByName(currentProjectName + suffix))
        .WhereNotNull()
        .ToArray();

      if (candidates.Length > 0)
      {
        if (candidates.Length == 1) return candidates[0];

        return null;
      }

      //Try to guess global test project
      candidates = solution.GetAllProjects()
        .Where(proj => TestProjectSuffixes.Any(suffix => proj.Name.EndsWith(suffix, StringComparison.Ordinal)))
        .ToArray();

      if (candidates.Length == 1) return candidates[0];

      return null;
    }

    [CanBeNull]
    private static string TryExtractProjectNameFromAssemblyMetadataAttribute([NotNull] IAttributeInstance attributeInstance)
    {
      const string testProjectKey = "ReSharperHelpers.TestProject";

      if (attributeInstance.PositionParameterCount != 2) return null;

      var key = attributeInstance.PositionParameter(0);
      if (key.IsConstant && key.ConstantValue.IsString() && string.Equals((string)key.ConstantValue.Value, testProjectKey, StringComparison.Ordinal))
      {
        var value = attributeInstance.PositionParameter(1);
        if (value.IsConstant && value.ConstantValue.IsString()) return (string)value.ConstantValue.Value;
      }

      return null;
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