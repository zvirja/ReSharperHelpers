using AlexPovar.ReSharperHelpers.ContextActions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class CopyFullClassNameActionTests : PathedContextActionExecuteTestBase<CopyFullClassNameAction>
  {
    [Test] public void TestFileIsNotChanged() { this.DoNamedTest2(); }

    [Test] public void TestFullClassNameIsCopied()
    {
      this.DoNamedTest2();

      var currentClipboardText = System.Windows.Forms.Clipboard.GetText();
      Assert.That(currentClipboardText, Is.EqualTo("AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.CopyFullClassNameActionTests.FullClassNameIsCopied, TestProject"));
    }

    [Test] public void TestInterfaceNameIsCopied()
    {
      this.DoNamedTest2();

      var currentClipboardText = System.Windows.Forms.Clipboard.GetText();
      Assert.That(currentClipboardText, Is.EqualTo("AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.CopyFullClassNameActionTests.InterfaceNameIsCopied, TestProject"));
    }
  }
}