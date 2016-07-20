using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions.ContainerNullability
{
  public abstract class ContainerNullabilityActionBase : TweaksContextActionBase
  {
    protected ContainerNullabilityActionBase([NotNull] ICSharpContextActionDataProvider provider)
    {
      this.Provider = provider;
      this.ItemCanBeNullShortName = CodeAnnotationsCache.ItemCanBeNullAttributeShortName;
      this.ItemNotNullShortName = CodeAnnotationsCache.ItemNotNullAttributeShortName;

      this.AttributeNames = new[] {this.ItemNotNullShortName, this.ItemCanBeNullShortName};
    }

    [NotNull]
    protected string ItemNotNullShortName { get; }

    [NotNull]
    protected string ItemCanBeNullShortName { get; }


    private IEnumerable<string> AttributeNames { get; }

    [NotNull]
    private ICSharpContextActionDataProvider Provider { get; }

    [NotNull]
    protected abstract string ThisAttributeShortName { get; }

    public bool LastIsAvailableResult { get; private set; }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var attributesOwner = this.Provider.GetSelectedElement<IAttributesOwnerDeclaration>();
      if (attributesOwner == null) return null;

      var annotationsCache = attributesOwner.GetPsiServices().GetCodeAnnotationsCache();

      AnnotationsUtil.RemoveSpecificAttributes(attributesOwner, this.AttributeNames, annotationsCache);

      var attributeType = annotationsCache.GetAttributeTypeForElement(attributesOwner, this.ThisAttributeShortName);
      if (attributeType == null) return null;

      var newAttribute = this.Provider.ElementFactory.CreateAttribute(attributeType);

      AnnotationsUtil.AddAnnotationAttribute(attributesOwner, newAttribute);

      return null;
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      this.LastIsAvailableResult = this.GetIsAvailable(cache);
      return this.LastIsAvailableResult;
    }

    private bool GetIsAvailable(IUserDataHolder cache)
    {
      var methodName = this.Provider.GetSelectedElement<ICSharpIdentifier>();
      if (methodName == null) return false;

      var parentNode = methodName.Parent;
      if (!(parentNode is IMethodDeclaration || parentNode is IPropertyDeclaration || parentNode is IFieldDeclaration || parentNode is IRegularParameterDeclaration)) return false;

      //Skip conditions, to not interfere with default nullability attributes
/*      var decl = parentNode as ICSharpDeclaration;
      if (IsContainerDeclarationFromReSharper(decl)) return false; */

      var typeOwner = (ITypeOwnerDeclaration) parentNode;
      var type = typeOwner.Type;

      if (!(type.IsGenericTask() || type.IsLazy() || IsIEnumerable(type))) return false;

      var annotationsCache = typeOwner.GetPsiServices().GetCodeAnnotationsCache();
      var attributesOwner = parentNode as IAttributesOwnerDeclaration;

      var alreadyDeclared = attributesOwner.AttributesEnumerable.Any(attr => annotationsCache.IsAnnotationAttribute(attr.GetAttributeInstance(), this.ThisAttributeShortName));

      if (alreadyDeclared) return false;

      return true;
    }

    private static bool IsIEnumerable([NotNull] IType type)
    {
      //Exclude string, because it doesn't make sense
      if (type.IsString()) return false;

      var typeElement = type.GetTypeElement();
      if (typeElement == null) return false;

      var predefinedTypes = type.Module.GetPredefinedType();
      var iEnumerableType = predefinedTypes.GenericIEnumerable.GetTypeElement();

      var interfaceType = type.GetInterfaceType();
      if (interfaceType != null && interfaceType.Equals(iEnumerableType)) return true;

      return typeElement.IsDescendantOf(iEnumerableType);
    }
  }
}