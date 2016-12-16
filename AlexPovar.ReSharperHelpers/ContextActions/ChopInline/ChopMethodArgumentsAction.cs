using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace AlexPovar.ReSharperHelpers.ContextActions.ChopInline
{
  public class ChopMethodArgumentsAction : ChopInlineMethodActionBase
  {
    public ChopMethodArgumentsAction([NotNull] ICSharpParametersOwnerDeclaration parametersOwnerDeclaration, [NotNull] IFormalParameterList paramList)
      : base(parametersOwnerDeclaration, paramList)
    {
    }


    public override string Text => "Chop method arguments";
  }
}