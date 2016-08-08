using AlexPovar.ReSharperHelpers.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.ContextActions
{
  public class ContainerNullabilityGroupActionTests : PathedContextActionExecuteTestBase<ContainerNullabilityGroupAction>
  {
    [Test] public void AddedToProperty() { this.DoNamedTest(); }

    [Test] public void AttributeMergedWithOther() { this.DoNamedTest(); }

    [Test] public void ItemCanBeNullAttributeAdded() { this.DoNamedTest(); }

    [Test] public void ItemNotNullAttributeAdded() { this.DoNamedTest(); }

    [Test] public void OppositeAttributeIsRemoved() { this.DoNamedTest(); }
  }


  public class ContainerNullabilityGroupActionAvailabilityTests : CSharpContextActionAvailabilityTestBase<ContainerNullabilityGroupAction>
  {
    protected override string ExtraPath => nameof(ContainerNullabilityGroupActionTests);

    [Test] public void Availability() { this.DoNamedTest(); }
  }
}