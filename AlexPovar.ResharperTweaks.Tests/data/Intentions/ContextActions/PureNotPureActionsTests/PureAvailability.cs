using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ResharperTweaks.Tests.data.Intentions.ContextActions.PureNotPureActionsTests
{
  class PureAvailability
  {
    void Void{off}Method()
    {
    }

    async Task Async{off}Method()
    {
      await Task.Delay(1);
    }

    string MethodWith{on}Result()
    {
      return null;
    }
  }
}
