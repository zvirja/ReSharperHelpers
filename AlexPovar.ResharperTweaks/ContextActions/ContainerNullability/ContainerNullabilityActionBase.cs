using System;
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
      Provider = provider;
      ItemCanBeNullShortName = CodeAnnotationsCache.ItemCanBeNullAttributeShortName;
      ItemNotNullShortName = CodeAnnotationsCache.ItemNotNullAttributeShortName;
    }


    [NotNull]
    protected string ItemNotNullShortName { get; }

    [NotNull]
    protected string ItemCanBeNullShortName { get; }

    [NotNull]
    private ICSharpContextActionDataProvider Provider { get; }

    [NotNull]
    protected abstract string ThisAttributeShortName { get; }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var attributesOwner = Provider.GetSelectedElement<IAttributesOwnerDeclaration>();
      if (attributesOwner == null) return null;

      var annotationsCache = attributesOwner.GetPsiServices().GetCodeAnnotationsCache();

      var candidatesToRemove = attributesOwner.AttributesEnumerable
        .Where(attr => annotationsCache.IsAnnotationAttribute(attr.GetAttributeInstance(), ItemCanBeNullShortName) ||
                       annotationsCache.IsAnnotationAttribute(attr.GetAttributeInstance(), ItemNotNullShortName))
        .ToList();

      candidatesToRemove.ForEach(c => attributesOwner.RemoveAttribute(c));

      var attributeType = annotationsCache.GetAttributeTypeForElement(attributesOwner, ThisAttributeShortName);
      if (attributeType == null) return null;

      var newAttribute = Provider.ElementFactory.CreateAttribute(attributeType);

      AnnotationsUtil.AddAnnotationAttribute(attributesOwner, newAttribute);

      return null;
    }


    public override bool IsAvailable(IUserDataHolder cache)
    {
      var methodName = Provider.GetSelectedElement<ICSharpIdentifier>();
      if (methodName == null) return false;

      var parentNode = methodName.Parent;
      if (!(parentNode is IMethodDeclaration || parentNode is IPropertyDeclaration || parentNode is IFieldDeclaration)) return false;

      //Skip conditions, to not interfere with default nullability attributes
/*      var decl = parentNode as ICSharpDeclaration;
      if (IsContainerDeclarationFromReSharper(decl)) return false; */

      var typeOwner = (ITypeOwnerDeclaration) parentNode;
      var type = typeOwner.Type;

      if (!(type.IsGenericTask() || type.IsLazy() || IsIEnumerable(type))) return false;

      var annotationsCache = typeOwner.GetPsiServices().GetCodeAnnotationsCache();
      var attributesOwner = parentNode as IAttributesOwnerDeclaration;

      var alreadyDeclared = attributesOwner.AttributesEnumerable.Any(attr => annotationsCache.IsAnnotationAttribute(attr.GetAttributeInstance(), ThisAttributeShortName));

      if (alreadyDeclared) return false;

      return true;
    }

    private static bool IsIEnumerable([NotNull] IType type)
    {
      var typeElement = type.GetTypeElement();
      if (typeElement == null) return false;

      var predefinedTypes = type.Module.GetPredefinedType();
      var iEnumerableType = predefinedTypes.GenericIEnumerable.GetTypeElement();

      var interfaceType = type.GetInterfaceType();
      if (interfaceType != null && interfaceType.Equals(iEnumerableType)) return true;

      return typeElement.IsDescendantOf(iEnumerableType);
    }

    // JetBrains.ReSharper.Intentions.CSharp.ContextActions.MarkNullableActionBase
    private static bool IsContainerDeclarationFromReSharper([CanBeNull] ICSharpDeclaration targetDeclaration)
    {
      var methodDeclaration = targetDeclaration as IMethodDeclaration;
      if (methodDeclaration != null)
      {
        return methodDeclaration.IsAsync || methodDeclaration.IsIterator;
      }
      var propertyDeclaration = targetDeclaration as IPropertyDeclaration;
      if (propertyDeclaration != null)
      {
        var accessor = propertyDeclaration.GetAccessor(AccessorKind.GETTER);
        return accessor != null && accessor.IsIterator;
      }
      return false;
    }
  }
}