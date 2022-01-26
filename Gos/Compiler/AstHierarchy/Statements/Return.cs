
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy {
    public class Return:AstNode, IStatement {
        private readonly ILogger<Return> _log;

        public Expression Expr { get; set; }

        public Return(ILogger<Return> logger = null) {
            _log = logger;
        }

        public override bool Validate(Context context) {
            if (Expr != null && !Expr.Validate(context)) {
                return false;
            }
            bool insideFun = false;

            for (var ctx = context; ctx != null; ctx = ctx.parent) {
                if (ctx.OpenFunction) {
                    insideFun = true;
                    break;
                }
            }
            if (!insideFun) {
                _log?.LogError(
                    "Line {l}, column {c}: return statement must be inside a function.",
                    Token.Line,
                    Token.Column);
            }
            return insideFun;
        }
    }
}