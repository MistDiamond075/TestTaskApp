using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskApp.actors
{
    public interface IActor
    {
        bool State { get; set; }
        DateTime LastUpdated { get; }
    }
}
