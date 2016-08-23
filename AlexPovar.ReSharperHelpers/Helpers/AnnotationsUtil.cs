using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace AlexPovar.ReSharperHelpers.Helpers
{
  public static class AnnotationsUtil
  {
    public static void CreateAndAddAnnotationAttribute([NotNull] IAttributesOwnerDeclaration ownerDeclaration, [NotNull] string newAttributeShortName)
    {
      var codeAnnotationConfig = ownerDeclaration.GetPsiServices().GetComponent<CodeAnnotationsConfiguration>();

      var pureAttributeType = codeAnnotationConfig.GetAttributeTypeForElement(ownerDeclaration, newAttributeShortName);
      if (pureAttributeType == null) return;

      var elementFactory = CSharpElementFactory.GetInstance(ownerDeclaration);
      var attribute = elementFactory.CreateAttribute(pureAttributeType);

      ownerDeclaration.AddAttributeAfter(attribute, null);
    }

    public static void RemoveSpecificAttributes(
      [NotNull] IAttributesOwnerDeclaration attributesOwner,
      [NotNull, ItemNotNull] IEnumerable<string> shortNames,
      [NotNull] CodeAnnotationsConfiguration codeAnnotationConfig)
    {
      var candidatesToRemove = attributesOwner.AttributesEnumerable
        .Where(attr => shortNames.Any(name => codeAnnotationConfig.IsAnnotationAttribute(attr.GetAttributeInstance(), name)))
        .ToList();

      candidatesToRemove.ForEach(attributesOwner.RemoveAttribute);
    }
  }
}