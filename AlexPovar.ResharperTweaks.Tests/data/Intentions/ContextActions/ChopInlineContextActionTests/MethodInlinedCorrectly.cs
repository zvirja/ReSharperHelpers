using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ResharperTweaks.Tests.data.Intentions.ContextActions.ChopInlineContextActionTests
{
  class MethodInlinedCorrectly
  {
    void Mixed {caret:One:line:method:arguments}Arguments(      int arg1,
 byte arg2,            string arg3,
         long arg4      )
    {
    }
  }
}
