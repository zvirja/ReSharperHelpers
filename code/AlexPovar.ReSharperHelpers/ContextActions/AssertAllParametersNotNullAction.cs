using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ReSharperHelpers.ContextActions
{
  [ContextAction(Group = "C#", Name = "[ReSharperHelpers] Assert all parameters are not null (or empty) action", Description = "Assert all parameter are not null or empty.", Priority = -1)]
  public class AssertAllParametersNotNullAction : AssertParameterNotNullAction
  {
    [CanBeNull] private IList<ICSharpParameterDeclaration> _myParameterDeclarations;

    public AssertAllParametersNotNullAction([NotNull] ICSharpContextActionDataProvider provider) : base(provider)
    {
    }

    public override string Text => "Assert all parameters are not null";

    public override bool IsAvailable(IUserDataHolder cache)
    {
      var selectedParameterDeclaration = this.GetSelectedParameterDeclaration();
      if (selectedParameterDeclaration == null) return false;

      if (!this.IsAvailableWithParameter(selectedParameterDeclaration)) return false;

      var multipleParameterDeclarations = GetMultipleParameterDeclarations(selectedParameterDeclaration);
      if (multipleParameterDeclarations == null) return false;

      var localList = default(LocalList<ICSharpParameterDeclaration>);
      foreach (var current in multipleParameterDeclarations)
      {
        if (current == null) return false;

        if (current == selectedParameterDeclaration || this.IsAvailableWithParameter(current))
        {
          localList.Add(current);
        }
      }

      if (localList.Count < 2)
      {
        return false;
      }

      this._myParameterDeclarations = localList.ResultingList();
      return true;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      if (this._myParameterDeclarations == null) return null;

      var list = new List<ICSharpStatement>();
      foreach (ICSharpParameterDeclaration current in this._myParameterDeclarations)
      {
        list.AddRange(this.ExecuteOverParameter(current));
      }

      return this.HandleAddedStatements(list.ToArray());
    }
  }
}