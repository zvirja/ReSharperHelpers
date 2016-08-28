using AlexPovar.ReSharperHelpers.ContextActions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class CopyFullClassNameActionTests : PathedContextActionExecuteTestBase<CopyFullClassNameAction>
  {
    [Test] public void FileIsNotChanged() { this.DoNamedTest(); }

    [Test] public void FullClassNameIsCopied()
    {
      this.DoNamedTest();

      var currentClipboardText = System.Windows.Forms.Clipboard.GetText();
      Assert.That(currentClipboardText, Is.EqualTo("AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.CopyFullClassNameActionTests.FullClassNameIsCopied, TestProject"));
    }
  }
}