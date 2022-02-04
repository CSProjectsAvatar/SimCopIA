using DataClassHierarchy;
using Microsoft.Extensions.Logging;

namespace Compiler.AstHierarchy.Statements {
    public abstract class BehavStatement : AstNode, IStatement {
        private ILogger<BehavStatement> _log;

        public BehavStatement(ILogger<BehavStatement> logger) {
            _log = logger;
        }

        public override bool Validate(Context context) {
            if (!context.IsInsideBehav()) {
                _log.LogError(
                    Helper.LogPref + "this statement must be inside a behav block.",
                    Token.Line,
                    Token.Column);
                return false;
            }
            return true;
        }
    }
}