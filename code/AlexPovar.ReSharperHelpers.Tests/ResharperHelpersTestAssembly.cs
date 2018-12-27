using System;
using AlexPovar.ReSharperHelpers.Tests;
using JetBrains.TestFramework;
using NUnit.Framework;

#pragma warning disable 618

[assembly: TestDataPathBase(@".\data")]
#pragma warning restore 618

// ReSharper disable once CheckNamespace

[SetUpFixture]
public class ReSharperHelpersTestAssembly : ExtensionTestEnvironmentAssembly<ResharperHelpersTestEnvironmentZone>
{
  public override void SetUp()
  {
    base.SetUp();
    
    // Workaround for shutdown issue:
    // https://github.com/Microsoft/dotnet/blob/master/releases/net472/KnownIssues/593963%20-%20WPF%20Exceptions%20during%20AppDomain%20or%20process%20shutdown.md
    AppContext.SetSwitch("Switch.MS.Internal.DoNotInvokeInWeakEventTableShutdownListener", true);
  }
}