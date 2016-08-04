using System;
using System.Collections.Generic;
using System.Linq;
using AlexPovar.ReSharperHelpers.Helpers;
using AlexPovar.ReSharperHelpers.Settings;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
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
using JetBrains.ReSharper.Psi.CSharp.Tree;
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
    private const string TemplateDescription = "[AlexPovarHelpers] TestFile";

    [NotNull] private readonly ICSharpContextActionDataProvider _myProvider;

    public CreateOrBrowseTestFileAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      if (provider == null) throw new ArgumentNullException(nameof(provider));

      this._myProvider = provider;

      this.IsEnabledForProject = ResolveIsEnabledForProvider(provider);
    }

    private bool IsEnabledForProject { get; }

    [CanBeNull]
    private IProjectFile ExistingProjectFile { get; set; }

    public void Execute(ISolution solution, ITextControl textControl)
    {
      if (this.ExistingProjectFile != null)
      {
        ShowProjectFile(solution, this.ExistingProjectFile);
        return;
      }

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

            var settingsStore = declaration.GetSettingsStore();

            var helperSettings = settingsStore.GetKey<ReSharperHelperSettings>(SettingsOptimization.OptimizeDefault);

            var projectName = helperSettings.TestsProjectName;
            if (projectName.IsNullOrEmpty())
            {
              MessageBox.ShowError($"The test project value is not configured.{Environment.NewLine}Specify project name in configuration.", "ReSharper Helpers");
              return;
            }

            var project = solution.GetProjectsByName(projectName).FirstOrDefault();
            if (project == null)
            {
              MessageBox.ShowError($"Unable to find '{projectName}' project. Ensure project name is correct", "ReSharper Helpers");
              return;
            }

            var testNamespaceParts = TrimDefaultProjectNamespace(declaration.GetProject(), declaredType.GetContainingNamespace().QualifiedName);
            var testFolderLocation = testNamespaceParts.Aggregate(project.Location, (current, part) => current.Combine(part));

            testFolder = project.GetOrCreateProjectFolder(testFolderLocation, cookie);
            if (testFolder == null) return;

            testFileName = MakeTestClassName(declaredType.ShortName) + ".cs";

            testFileTemplate =
              StoredTemplatesProvider.Instance.EnumerateTemplates(settingsStore, TemplateApplicability.File).FirstOrDefault(t => t.Description == TemplateDescription);
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

    public string Text => this.ExistingProjectFile == null ? "[Helpers] Create test file" : "[Helpers] Go to test file";

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      return this.ToContextAction(HelperActionsConstants.ContextActionsAnchor, MyIcons.ContextActionIcon);
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      this.ExistingProjectFile = null;
      if (!this.IsEnabledForProject) return false;

      var typeDeclaration = this._myProvider.GetSelectedElement<ICSharpTypeDeclaration>();
      if (typeDeclaration == null) return false;

      //Disable for nested classes
      if (typeDeclaration.GetContainingTypeDeclaration() != null) return false;

      var declaredElement = typeDeclaration.DeclaredElement;
      if (declaredElement == null) return false;

      //TRY RESOLVE EXISTING TEST
      var symbolScope = typeDeclaration.GetPsiServices().Symbols.GetSymbolScope(LibrarySymbolScope.NONE, true);

      var typeName = declaredElement.ShortName;
      var alreadyDeclaredClasses = symbolScope.GetElementsByShortName(MakeTestClassName(typeName)).OfType<IClass>().Where(c => c != null).ToArray();
      if (alreadyDeclaredClasses.Length == 0) return true;

      var myProject = typeDeclaration.GetProject();
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
      ;

      return true;
    }

    private static string MakeTestClassName(string className)
    {
      return className + "Tests";
    }

    private static bool ResolveIsEnabledForProvider([NotNull] ICSharpContextActionDataProvider provider)
    {
      var project = provider.Project;
      if (project == null) return false;

      var dataContext = project.ToDataContext();
      var contextRange = ContextRange.Smart(dataContext);

      var settingsStore = Shell.Instance.GetComponent<SettingsStore>();

      var settingsStoreBound = settingsStore.BindToContextTransient(contextRange);
      var mySettings = settingsStoreBound.GetKey<ReSharperHelperSettings>(SettingsOptimization.OptimizeDefault);

      return !project.Name.Equals(mySettings.TestsProjectName);
    }


    [NotNull]
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

    private static void ShowProjectFile([NotNull] ISolution solution, [NotNull] IProjectFile file)
    {
      var editor = solution.GetComponent<IEditorManager>();
      editor.OpenProjectFile(file, true);
    }
  }
}