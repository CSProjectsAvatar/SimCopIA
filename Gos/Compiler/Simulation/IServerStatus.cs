using ServersWithLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Simulation {
    interface IServerStatus {
        List<object> AcceptedReqs { get; }
        bool CanProcess { get; }
    }
}
