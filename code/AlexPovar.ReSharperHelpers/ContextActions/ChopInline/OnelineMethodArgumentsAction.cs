using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace AlexPovar.ReSharperHelpers.ContextActions.ChopInline
{
  public class OnelineMethodArgumentsAction : ChopInlineMethodActionBase
  {
    public OnelineMethodArgumentsAction([NotNull] ICSharpParametersOwnerDeclaration parametersOwnerDeclaration, [NotNull] IFormalParameterList paramList)
      : base(parametersOwnerDeclaration, paramList)
    {
    }


    public override string Text => "One line method arguments";

    protected override void DoPutNewIndents(IFormalParameterList parameters)
    {
      //Indentations were removed. We just not put the new ones.
    }
  }
}