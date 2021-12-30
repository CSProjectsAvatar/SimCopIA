

using Compiler;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace DataClassHierarchy
{
    public abstract class Connection:Expression
    {
        public string LeftAgent;
        public IEnumerable<string> Agents { get; set; }
        private readonly ILogger<Connection> _log;

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
                
                var varType = Helper.GetType(context.GetVar(serv));
                if (varType is not GosType.Server) {
                    _log?.LogError(
                        "Line {line}, column {col}: variable '{serv}' has to be of type Server.",
                        Token.Line,
                        Token.Column,
                        serv);
                    return false;
                }
            }
            
            
        }
    }
}