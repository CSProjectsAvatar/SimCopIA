using Agents;
using Compiler;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace DataClassHierarchy
{
    public abstract class Connection:AstNode, IStatement  // @todo MOVER ESTAS CLASES PA LA CARPETA D STATEMENTS
    {
        public string LeftAgent;
        public IEnumerable<string> Agents { get; set; }
        protected readonly ILogger<Connection> _log;

        public Connection(ILogger<Connection> logger = null) {
            _log = logger;
        }
        public abstract (bool, object) TryCompute(Agent left, List<Agent> agents);

        public override bool Validate(Context context)
        {
            foreach (var serv in Agents)
            {
                if (!context.CheckVar(serv)) {
                    _log?.LogError(
                        "Line {line}, column {col}: variable '{id}' not defined.",
                        Token.Line,
                        Token.Column,
                        serv);
                    return false;
                }
            }
            return true;
        }
    }
}