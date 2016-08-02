using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.ContainerNullabilityActionTests
{
  class ItemNotNullAttributeAdded
  {
    IEnumerable<string> Get{caret:Item:not:null}Results()
    {
      return Enumerable.Empty<string>();
    }
  }
}
