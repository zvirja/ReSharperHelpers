using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.ContainerNullabilityActionTests
{
  class AddedToProperty
  {
    private IEnumerable<string> Some{caret:Item:not:null}Property { get; }
  }
}
