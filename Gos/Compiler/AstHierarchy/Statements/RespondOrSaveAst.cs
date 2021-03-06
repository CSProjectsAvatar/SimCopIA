using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class RespondOrSaveAst : AstNode, IStatement {
        private ILogger<RespondOrSaveAst> _log;

        public RespondOrSaveAst(ILogger<RespondOrSaveAst> logger) {
            _log = logger;
        }

        public Expression Request { get; init; }

        public override bool Validate(Context context) {
            if (!context.IsInsideBehav()) {
                _log.LogError(
                    Helper.LogPref + "this statement must be inside a behav block.",
                    Token.Line,
                    Token.Column);
                return false;
            }
            return Request.Validate(context);
        }
    }
}
