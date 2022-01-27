
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
            bool inside = false;
            bool inBehav = false;

            for (var ctx = context; ctx != null; ctx = ctx.parent) {
                if (ctx.OpenFunction || ctx.OpenBehavior) {
                    inside = true;
                    inBehav = ctx.OpenBehavior;
                    break;
                }
            }
            if (!inside) {
                _log?.LogError(
                    "Line {l}, column {c}: return statement must be inside a function or behavior.",
                    Token.Line,
                    Token.Column);
            } else if (inBehav && Expr != null) {
                _log?.LogError(
                    "Line {l}, column {c}: no expression can be returned in a behavior.",
                    Token.Line,
                    Token.Column);
                return false;
            }
            return inside;
        }
    }
}