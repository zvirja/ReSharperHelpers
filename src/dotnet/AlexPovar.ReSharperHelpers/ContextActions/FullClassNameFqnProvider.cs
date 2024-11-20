using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Features.Environment.CopyFqn;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  // Instantiation option was looked at other IFqnProvider implementations at a time
  [SolutionComponent(Instantiation.DemandAnyThreadSafe)]
  public class FullClassNameFqnProvider : IFqnProvider
  {
    public int Priority => -1;

    public bool IsApplicable(IDataContext dataContext)
    {
      return dataContext.GetData(PsiDataConstants.DECLARED_ELEMENTS)?.Any(x => x is ITypeElement) == true;
    }

    public IEnumerable<PresentableFqn> GetSortedFqns(IDataContext dataContext)
    {
      IEnumerable<ITypeElement> typeElements = dataContext.GetData(PsiDataConstants.DECLARED_ELEMENTS)?.OfType<ITypeElement>();
      if (typeElements == null) yield break;

      foreach (ITypeElement typeElement in typeElements)
      {
        string text = RenderFqn(typeElement);

        var iconManager = typeElement.GetSolution().GetComponent<PsiIconManager>();
        IconId icon = iconManager.GetImage(typeElement, typeElement.PresentationLanguage, drawExtensions: false);

        yield return new PresentableFqn(icon, new RichText(text));
      }
    }

    private static string RenderFqn(ITypeElement declaredElement)
    {
      string typeName = declaredElement.GetClrName().FullName;
      string moduleName = declaredElement.Module.DisplayName;

      if (declaredElement.Module is IProjectPsiModule projectModule)
      {
        IProject proj = projectModule.Project;
        moduleName = proj.GetOutputAssemblyName(proj.GetCurrentTargetFrameworkId());
      }

      var fullName = $"{typeName}, {moduleName}";
      return fullName;
    }
  }
}
