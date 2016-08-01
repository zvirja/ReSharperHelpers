using AlexPovar.ResharperTweaks.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ResharperTweaks.Tests.ContextActions
{
  public class ContainerNullabilityGroupActionTests : PathedContextActionExecuteTestBase<ContainerNullabilityGroupAction>
  {
    [TestCase("ItemNotNullAttributeAdded.cs")]
    [TestCase("ItemCanBeNullAttributeAdded.cs")]
    [TestCase("AttributeMergedWithOther.cs")]
    [TestCase("OppositeAttributeIsRemoved.cs")]
    [TestCase("AddedToProperty.cs")]
    public void Run(string testName)
    {
      this.DoTestFiles(testName);
    }
  }


  public class ContainerNullabilityGroupActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<ContainerNullabilityGroupAction>
  {
    protected override string ExtraPath => nameof(ContainerNullabilityGroupActionTests);

    [Test]
    public void Run()
    {
      this.DoTestFiles("Availability.cs");
    }
  }
}