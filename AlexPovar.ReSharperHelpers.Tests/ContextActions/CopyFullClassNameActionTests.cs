using AlexPovar.ReSharperHelpers.ContextActions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
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