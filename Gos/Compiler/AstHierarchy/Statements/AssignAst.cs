using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class AssignAst : AstNode, IStatement {
        private ILogger<AssignAst> _log;

        public AssignAst(ILogger<AssignAst> logger) {
            _log = logger;
        }

        public Expression Left { get; init; }
        public Expression NewVal { get; init; }

        public override bool Validate(Context context) {
            if (!(Left is Variable or ListIdxGetAst or PropGetAst)) {
                _log.LogError(
                    Helper.LogPref + "left expression must be a l-value: identifier, list accessor or property accessor.",
                    Token.Line,
                    Token.Column);
                return false;
            }
            return Left.Validate(context) && NewVal.Validate(context);
        }
    }
}
