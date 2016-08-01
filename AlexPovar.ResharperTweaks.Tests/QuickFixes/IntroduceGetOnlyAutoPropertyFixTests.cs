using AlexPovar.ResharperTweaks.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ResharperTweaks.Tests.QuickFixes
{
  public class IntroduceGetOnlyAutoPropertyFixTests : CSharpQuickFixTestBase<IntroduceGetOnlyAutoPropertyFix>
  {
    [TestCase("AnnotationsAreCopied.cs")]
    public void RunIntroduceGetOnlyAutoPropertyFixTest(string testName)
    {
      this.DoTestFiles(testName);
    }
  }
}