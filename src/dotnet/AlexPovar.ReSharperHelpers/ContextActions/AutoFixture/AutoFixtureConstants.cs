using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;

namespace AlexPovar.ReSharperHelpers.ContextActions.AutoFixture;

public static class AutoFixtureConstants
{
  [NotNull] public static readonly IClrTypeName FrozenAttributeType = new ClrTypeName("AutoFixture.Xunit2.FrozenAttribute");

  [NotNull] public static readonly IClrTypeName GreedyAttributeType = new ClrTypeName("AutoFixture.Xunit2.GreedyAttribute");

  [NotNull] public static readonly IClrTypeName MatchingEnumType = new ClrTypeName("AutoFixture.Xunit2.Matching");
}
