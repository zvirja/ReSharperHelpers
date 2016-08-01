using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPovar.ResharperTweaks.Tests.data.Intentions.ContextActions.ContainerNullabilityActionTests
{
  class Availability
  {
    void Void{off}Method()
    {
    }

    Task Task{off}Method()
    {
      return Task.Delay(1);
    }

    string String{off}Method()
    {
      return "hello";
    }

    int Int{off}Method()
    {
      return 10;
    }
    
    Task<int> Result{on}TaskMethod()
    {
      return Task.FromResult(1);
    }

    IEnumerable<string> Enumerable{on}Method()
    {
      return Enumerable.Empty<string>();
    }

    List<bool> List{on}Method()
    {
      return new List<bool>();
    }
    
    Lazy<int> Lazy{on}Method()
    {
      return new Lazy<int>();
    }
  }
}
