using System.Threading;
using AlexPovar.ReSharperHelpers.Tests;
using JetBrains.TestFramework;
using NUnit.Framework;

#pragma warning disable 618

[assembly: Apartment(ApartmentState.STA)]

#pragma warning restore 618

// ReSharper disable once CheckNamespace

[SetUpFixture]
public class ReSharperHelpersTestAssembly : ExtensionTestEnvironmentAssembly<IReSharperHelpersTestEnvironmentZone>
{
}
