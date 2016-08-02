using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace AlexPovar.ReSharperHelpers.ContextActions.ChopInline
{
  public class ChopMethodArgumentsAction : ChopInlineMethodActionBase
  {
    public ChopMethodArgumentsAction([NotNull] ICSharpParametersOwnerDeclaration methodDeclaration)
      : base(methodDeclaration)
    {
    }

    public override string Text => "Chop method arguments";
  }
}