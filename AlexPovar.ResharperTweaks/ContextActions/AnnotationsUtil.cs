using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace AlexPovar.ResharperTweaks.ContextActions
{
  public static class AnnotationsUtil
  {
    public static void AddAnnotationAttribute([NotNull] IAttributesOwnerDeclaration ownerDeclaration, [NotNull] IAttribute newAttribute)
    {
      var codeAnnotationsCache = ownerDeclaration.GetPsiServices().GetCodeAnnotationsCache();

      var lastAnnotationAttribute = ownerDeclaration.AttributesEnumerable.LastOrDefault(attr =>
      {
        var attrInstanceType = attr.GetAttributeInstance().GetClrName();
        return codeAnnotationsCache.IsAnnotationType(attrInstanceType, attrInstanceType.ShortName);
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
  }
}