using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture
{
  public static class AutoFixtureConstants
  {
    [NotNull] public static readonly IClrTypeName FrozenAttributeType = new ClrTypeName("Ploeh.AutoFixture.Xunit2.FrozenAttribute");

    [NotNull] public static readonly IClrTypeName GreedyAttributeType = new ClrTypeName("Ploeh.AutoFixture.Xunit2.GreedyAttribute");

    [NotNull] public static readonly IClrTypeName MathingEnumType = new ClrTypeName("Ploeh.AutoFixture.Xunit2.Matching");
  }
}