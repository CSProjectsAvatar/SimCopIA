using DataClassHierarchy;
using Microsoft.Extensions.Logging;

namespace Compiler.AstHierarchy.Statements {
    public abstract class BehavStatement : AstNode, IStatement {
        private ILogger<BehavStatement> _log;

        public BehavStatement(ILogger<BehavStatement> logger) {
            _log = logger;
        }

        public override bool Validate(Context context) {
            return Validate(context, _log, Token);
        }

        internal static bool Validate(Context context, ILogger logger, Token token) {
            if (!context.IsInsideBehav()) {
                logger.LogError(
                    Helper.LogPref + "this statement must be inside a behav block.",
                    token.Line,
                    token.Column);
                return false;
            }
            return true;
        }
    }
}