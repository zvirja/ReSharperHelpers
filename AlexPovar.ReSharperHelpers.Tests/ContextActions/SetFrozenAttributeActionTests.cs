using AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class SetFrozenAttributeActionTests : PathedContextActionExecuteTestBase<SetFrozenAttributeAction>
  {
    [Test] public void TestFrozenByExactTypeIsArranged() { this.DoNamedTest2(); }
    [Test] public void TestSimpleFrozenIsArranged() { this.DoNamedTest2(); }

    /*protected override CaretPositionsProcessor CreateCaretPositionProcessor(FileSystemPath temporaryDirectory)
    {
      return new BraceSupportingCaretPositionsProcessor(temporaryDirectory);
    }

    private class BraceSupportingCaretPositionsProcessor : CaretPositionsProcessor
    {
      public BraceSupportingCaretPositionsProcessor([NotNull] FileSystemPath temporaryDirectory) : base(temporaryDirectory)
      {
      }

      protected override bool IsValidNameChar(char c, bool first)
      {
        if (!first && (c == '[' || c == ']')) return true;

        return base.IsValidNameChar(c, first);
      }


      public override void Process(FileSystemPath basePath, params string[] files)
      {
        base.Process(basePath, files);

      }

      protected override bool IsValidPositionName(string name)
      {
        return base.IsValidPositionName(name);
        
      }
    }
*/
  }

  public class SetFrozenAttributeActionAvailabilityTest : CSharpContextActionAvailabilityTestBase<SetFrozenAttributeAction>
  {
    protected override string ExtraPath => nameof(SetFrozenAttributeActionTests);

    [Test] public void TestAvailability1() { this.DoNamedTest2(); }
    [Test] public void TestAvailability2() { this.DoNamedTest2(); }
  }
}