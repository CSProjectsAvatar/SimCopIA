using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class BreakAst : AstNode, IStatement {
        private readonly ILogger<BreakAst> _log;

        public BreakAst(ILogger<BreakAst> logger) {
            _log = logger;
        }

        public override bool Validate(Context context) {
            bool insideLoop = false;

            for (var ctx = context; ctx != null; ctx = ctx.parent) {
                if (ctx.OpenLoop) {
                    insideLoop = true;
                    break;
                }
            }
            if (!insideLoop) {
                _log.LogError(
                    "Line {l}, column {c}: break statement must be inside a loop.",
                    Token.Line,
                    Token.Column);
            }
            return insideLoop;
        }
    }
}
