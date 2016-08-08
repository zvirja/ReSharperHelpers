using AlexPovar.ReSharperHelpers.ContextActions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class CopyFullClassNameActionTests : PathedContextActionExecuteTestBase<CopyFullClassNameAction>
  {
    [Test] public void FileIsNotChanged() { this.DoNamedTest(); }
  }
}