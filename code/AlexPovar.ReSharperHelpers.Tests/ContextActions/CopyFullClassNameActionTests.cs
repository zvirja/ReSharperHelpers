using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.Application.Components;
using JetBrains.Application.UI.Components;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class CopyFullClassNameActionTests : PathedContextActionExecuteTestBase<CopyFullClassNameAction>
  {
    [Test] public void TestFileIsNotChanged() { this.DoNamedTest2(); }

    [Test] public void TestFullClassNameIsCopied()
    {
      this.DoNamedTest2();

      var currentClipboardText = this.ShellInstance.GetComponent<Clipboard>().GetText();
      Assert.That(currentClipboardText, Is.EqualTo("AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.CopyFullClassNameActionTests.FullClassNameIsCopied, TestProject"));
    }

    [Test] public void TestInterfaceNameIsCopied()
    {
      this.DoNamedTest2();

      var currentClipboardText = this.ShellInstance.GetComponent<Clipboard>().GetText();
      Assert.That(currentClipboardText, Is.EqualTo("AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.CopyFullClassNameActionTests.InterfaceNameIsCopied, TestProject"));
    }
  }
}