using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.ContainerNullabilityActionTests
{
  class ItemCanBeNullAttributeAdded
  {
    IEnumerable<string> Get{caret:Item:can:be:null}Results()
    {
      return Enumerable.Empty<string>();
    }
  }
}
