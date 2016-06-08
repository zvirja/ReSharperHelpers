using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public class ChopMethodArgumentsAction : ChopInlineMethodActionBase
  {
    public ChopMethodArgumentsAction([NotNull] ICSharpParametersOwnerDeclaration methodDeclaration)
      : base(methodDeclaration)
    {
    }

    public override string Text => "[Tweaks] Chop method arguments";
  }
}