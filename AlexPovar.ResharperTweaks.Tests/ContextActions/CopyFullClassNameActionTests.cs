using AlexPovar.ResharperTweaks.ContextActions;
using NUnit.Framework;

namespace AlexPovar.ResharperTweaks.Tests.ContextActions
{
  public class CopyFullClassNameActionTests : PathedContextActionExecuteTestBase<CopyFullClassNameAction>
  {
    [TestCase("FileIsNotChanged.cs")]
    public void RunCopyFullClassNameActionTest(string testName)
    {
      this.DoTestFiles(testName);
    }
  }
}