using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.AssertParametersNotNullActionTests
{
  class AssertAllWorksCorrectly
  {
    void TestMethod(object ar{caret:Assert:all:parameters:are:not:null}g1, int arg2, string arg3)
    {
    }
  }
}