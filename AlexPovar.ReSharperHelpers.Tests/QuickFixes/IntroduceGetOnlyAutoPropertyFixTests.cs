using AlexPovar.ReSharperHelpers.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.QuickFixes
{
  public class IntroduceGetOnlyAutoPropertyFixTests : CSharpQuickFixTestBase<IntroduceGetOnlyAutoPropertyFix>
  {
    [Test] public void AnnotationsAreCopied() { this.DoNamedTest(); }
  }
}