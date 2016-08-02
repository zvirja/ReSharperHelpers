using AlexPovar.ReSharperHelpers.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.QuickFixes
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