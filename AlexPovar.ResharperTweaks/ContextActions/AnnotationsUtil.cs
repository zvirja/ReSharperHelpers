using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public static class AnnotationsUtil
  {
    private static readonly IEnumerable<string> NullabilityAttributes = new[] {CodeAnnotationsCache.NotNullAttributeShortName, CodeAnnotationsCache.CanBeNullAttributeShortName};

    public static void AddAnnotationAttribute([NotNull] IAttributesOwnerDeclaration ownerDeclaration, [NotNull] IAttribute newAttribute,
      [CanBeNull] CodeAnnotationsCache codeAnnotationCache = null)
    {
      codeAnnotationCache = codeAnnotationCache ?? ownerDeclaration.GetPsiServices().GetCodeAnnotationsCache();

      var lastAnnotationAttribute = ownerDeclaration.AttributesEnumerable.LastOrDefault(attr =>
      {
        var attrInstanceType = attr.GetAttributeInstance().GetClrName();
        return codeAnnotationCache.IsAnnotationType(attrInstanceType, attrInstanceType.ShortName);
      });

      if (lastAnnotationAttribute == null)
      {
        ownerDeclaration.AddAttributeAfter(newAttribute, null);
      }
      else
      {
        var attrList = (IAttributeList) lastAnnotationAttribute.Parent.NotNull("Attribute parent cannot be null.");
        attrList.AddAttributeAfter(newAttribute, lastAnnotationAttribute);
      }
    }

    public static void CreateAndAddAnnotationAttribute([NotNull] IAttributesOwnerDeclaration owner,
      [NotNull] string newAttributeShortName)
    {
      var codeAnnotationCache = owner.GetPsiServices().GetCodeAnnotationsCache();

      var pureAttributeType = codeAnnotationCache.GetAttributeTypeForElement(owner, newAttributeShortName);
      if (pureAttributeType == null) return;

      var elementFactory = CSharpElementFactory.GetInstance(owner);
      var attribute = elementFactory.CreateAttribute(pureAttributeType);

      AddAnnotationAttribute(owner, attribute, codeAnnotationCache);
    }

    public static void RemoveSpecificAttributes(IAttributesOwnerDeclaration attributesOwner, IEnumerable<string> shortNames, CodeAnnotationsCache cache)
    {
      var candidatesToRemove = attributesOwner.AttributesEnumerable
        .Where(attr => shortNames.Any(name => cache.IsAnnotationAttribute(attr.GetAttributeInstance(), name)))
        .ToList();

      candidatesToRemove.ForEach(attributesOwner.RemoveAttribute);
    }

    public static void RemoveNullabilityAttributes(IAttributesOwnerDeclaration attributesOwner, CodeAnnotationsCache cache)
    {
      RemoveSpecificAttributes(attributesOwner, NullabilityAttributes, cache);
    }
  }
}