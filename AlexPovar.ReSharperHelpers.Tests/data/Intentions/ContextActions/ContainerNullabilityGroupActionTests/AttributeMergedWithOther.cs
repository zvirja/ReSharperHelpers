using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.ContainerNullabilityActionTests
{
  class AttributeMergedWithOther
  {
    [NotNull]
    IEnumerable<string> Get{caret:Item:not:null}Result()
    {
      return Enumerable.Empty<string>();
    }
  }
}
