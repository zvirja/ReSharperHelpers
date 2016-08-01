using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  [ContextAction(Group = "C#", Name = "[Tweaks] Set ItemCanBeNull/ItemNotNull attribute", Description = "Sets ItemCanBeNull/ItemNotNull annotation attribute.")]
  public class ContainerNullabilityGroupAction : IContextAction
  {
    public ContainerNullabilityGroupAction([NotNull] ICSharpContextActionDataProvider provider)
    {
      this.Provider = provider;

      this.ItemCanBeNullShortName = CodeAnnotationsCache.ItemCanBeNullAttributeShortName;
      this.ItemNotNullShortName = CodeAnnotationsCache.ItemNotNullAttributeShortName;

      this.AttributeNames = new[] {this.ItemNotNullShortName, this.ItemCanBeNullShortName};
    }

    [NotNull]
    private ICSharpContextActionDataProvider Provider { get; }

    [NotNull]
    private IEnumerable<string> AttributeNames { get; }

    private bool ItemNotNullActionAvailable { get; set; }

    private bool ItemCanBeNullActionAvailable { get; set; }

    [NotNull]
    private string ItemNotNullShortName { get; }

    [NotNull]
    private string ItemCanBeNullShortName { get; }

    public IEnumerable<IntentionAction> CreateBulbItems()
    {
      var anchor = MyUtil.CreateTweaksGroupAnchor();

      if (this.ItemNotNullActionAvailable)
      {
        yield return new ActionWrapper(this.ItemNotNullShortName, "Item not null", this).ToTweaksAnnotateAction(anchor);
      }

      if (this.ItemCanBeNullActionAvailable)
      {
        yield return new ActionWrapper(this.ItemCanBeNullShortName, "Item can be null", this).ToTweaksAnnotateAction(anchor);
      }
    }

    public bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = this.Provider.GetSelectedElement<ICSharpIdentifier>();
      if (methodName == null) return false;

      var parentNode = methodName.Parent;
      if (!(parentNode is IMethodDeclaration || parentNode is IPropertyDeclaration || parentNode is IFieldDeclaration || parentNode is IRegularParameterDeclaration)) return false;

      var typeOwner = (ITypeOwnerDeclaration) parentNode;
      var type = typeOwner.Type;

      if (!(type.IsGenericTask() || type.IsLazy() || IsIEnumerable(type))) return false;

      var annotationsCache = typeOwner.GetPsiServices().GetCodeAnnotationsCache();
      var attributesOwner = parentNode as IAttributesOwnerDeclaration;

      this.ItemNotNullActionAvailable = !AnnotationsUtil.IsAnnotationAttributePresent(this.ItemNotNullShortName, attributesOwner, annotationsCache);
      this.ItemCanBeNullActionAvailable = !AnnotationsUtil.IsAnnotationAttributePresent(this.ItemCanBeNullShortName, attributesOwner, annotationsCache);

      return this.ItemNotNullActionAvailable || this.ItemCanBeNullActionAvailable;
    }


    private Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress, string attributeShortName)
    {
      var attributesOwner = this.Provider.GetSelectedElement<IAttributesOwnerDeclaration>();
      if (attributesOwner == null) return null;

      var annotationsCache = attributesOwner.GetPsiServices().GetCodeAnnotationsCache();

      AnnotationsUtil.RemoveSpecificAttributes(attributesOwner, this.AttributeNames, annotationsCache);

      var attributeType = annotationsCache.GetAttributeTypeForElement(attributesOwner, attributeShortName);
      if (attributeType == null) return null;

      var newAttribute = this.Provider.ElementFactory.CreateAttribute(attributeType);

      AnnotationsUtil.AddAnnotationAttribute(attributesOwner, newAttribute);

      return null;
    }

    private static bool IsIEnumerable([NotNull] IType type)
    {
      //Exclude string, because it doesn't make sense
      if (type.IsString()) return false;

      if (type is IArrayType) return true;

      var typeElement = type.GetTypeElement();
      if (typeElement == null) return false;

      var predefinedTypes = type.Module.GetPredefinedType();
      var iEnumerableType = predefinedTypes.GenericIEnumerable.GetTypeElement();

      var interfaceType = type.GetInterfaceType();
      if (interfaceType != null && interfaceType.Equals(iEnumerableType)) return true;

      return typeElement.IsDescendantOf(iEnumerableType);
    }

    private class ActionWrapper : BulbActionBase
    {
      public ActionWrapper([NotNull] string annotationAttributeName, [NotNull] string text, [NotNull] ContainerNullabilityGroupAction parentAction)
      {
        this.AnnotationAttributeName = annotationAttributeName;
        this.Text = text;
        this.ParentAction = parentAction;
      }

      [NotNull]
      private string AnnotationAttributeName { get; }

      public override string Text { get; }

      [NotNull]
      private ContainerNullabilityGroupAction ParentAction { get; }

      protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
      {
        return this.ParentAction.ExecutePsiTransaction(solution, progress, this.AnnotationAttributeName);
      }
    }
  }
}