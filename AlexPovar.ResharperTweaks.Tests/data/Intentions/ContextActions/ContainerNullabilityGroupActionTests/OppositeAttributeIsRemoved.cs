using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ResharperTweaks.Tests.data.Intentions.ContextActions.ContainerNullabilityActionTests
{
  class OppositeAttributeIsRemoved
  {
    [ItemCanBeNull]
    IEnumerable<string> Get{caret:Item:not:null}Result()
    {
      return Enumerable.Empty<string>();
    }
  }
}
