

using Compiler;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace DataClassHierarchy
{   // ->
    public class RightConn:Connection
    {

        public RightConn(ILogger<RightConn> logger = null) {
            _log = logger;
        }
        public bool TryCompute(Agent left, List<Agent> agents){
            left.AddAgents(agents);
            return true;
        }
        

    }
}