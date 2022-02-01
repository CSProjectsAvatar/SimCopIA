
using Compiler;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DataClassHierarchy {
    public class LetVar:AstNode, IStatement {
        public string Identifier { get; set; }
        public Expression Expr { get; set; }
        private ILogger<LetVar> _log;

        public LetVar(ILogger<LetVar> logger = null) {
            _log = logger;
        }
        
        public override bool Validate(Context context) {
            var forbiddenNames = new[] { Helper.HiddenDoneReqsHeapVar };
            if (forbiddenNames.Contains(Identifier)) {
                _log?.LogError(
                    Helper.LogPref + "naming a variable '{id}' is forbidden.",
                    Token.Line,
                    Token.Column,
                    Identifier);
                return false;
            }
            var exprValid = Expr.Validate(context);
            if (exprValid && !context.DefVariable(Identifier)) {
                _log?.LogError(
                    "Line {line}, column {col}: variable '{id}' already defined.",
                    Token.Line,
                    Token.Column,
                    Identifier);
                return false;
            }
            return exprValid;
        }
    }
}