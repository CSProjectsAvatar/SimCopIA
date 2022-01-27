using Agents;
using Compiler;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace DataClassHierarchy {   
    /// <summary>
    /// ->
    /// </summary>
    public class RightConn : Connection {
        private ILogger<RightConn> _log;

        public RightConn(ILogger<RightConn> logger = null) : base(logger) {
            _log = logger;
        }
        public override (bool, object) TryCompute(Agent left, List<Agent> servers) {
            if (left is DistributionServer ds) {
                ds.SetWorkers(servers.Select(s => s.ID).ToList());
                return (true, ds);
            }
            _log?.LogError(
                "Line {l}, column {c}: left operand must be a distribution server, but it's a {type} instead.",
                Token.Line,
                Token.Column,
                left.GetType().Name);
            return (false, default);
        }
    }
}