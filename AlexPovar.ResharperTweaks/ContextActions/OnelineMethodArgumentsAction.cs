using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public class OnelineMethodArgumentsAction : ChopInlineMethodActionBase
  {
    public OnelineMethodArgumentsAction([NotNull] IMethodDeclaration methodDeclaration) : base(methodDeclaration)
    {
      //see ChangeModifierAction 
    }

    public override string Text => "One line method arguments";

    protected override void DoPutNewIndents(IFormalParameterList parameters)
    {
      //Do nothing
    }
  }
}