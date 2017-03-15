using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace AlexPovar.ReSharperHelpers.Tests.data.Intentions.ContextActions.PureNotPureActionsTests
{
  class PureAttributeIsAdded
  {
    [Pure]
    string Met{caret}hod()
    {
    }
  }
}
