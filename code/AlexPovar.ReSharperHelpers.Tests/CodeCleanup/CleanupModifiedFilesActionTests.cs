using AlexPovar.ReSharperHelpers.CodeCleanup;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Features.Altering.CodeCleanup;
using NUnit.Framework;

namespace AlexPovar.ReSharperHelpers.Tests.CodeCleanup
{
  public class CleanupModifiedFilesActionTests
  {
    [Test] public void WpfDialogMethodIsPresent()
    {
      //act
      var result = CleanupModifiedFilesAction.GetSelectProfileWithWpfDialogMethod();

      //assert
      Assert.That(result, Is.Not.Null);
    }

    [Test] public void WpfDialogMethodIsValid()
    {
      //arrange
      var method = CleanupModifiedFilesAction.GetSelectProfileWithWpfDialogMethod();
      var methodParams = method.GetParameters();

      //act
      var retType = method.ReturnType;

      //assert
      Assert.That(retType, Is.EqualTo(typeof(CodeCleanupProfile)));

      Assert.That(methodParams.Length, Is.EqualTo(3));
      Assert.That(methodParams[0].ParameterType, Is.EqualTo(typeof(CodeCleanupFilesCollector)));
      Assert.That(methodParams[1].ParameterType, Is.EqualTo(typeof(bool)));
      Assert.That(methodParams[2].ParameterType, Is.EqualTo(typeof(IDataContext)));
    }
  }
}