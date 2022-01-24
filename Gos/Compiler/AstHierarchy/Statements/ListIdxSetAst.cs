using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class ListIdxSetAst : AstNode, IStatement {
        private ILogger<VarAssign> _log;

        public ListIdxSetAst(ILogger<VarAssign> logger) {
            _log = logger;
        }

        public string RootListName { get; init; }
        public IEnumerable<Expression> Idxs { get; init; }
        public Expression NewValueExpr { get; init; }

        public override bool Validate(Context context) {
            if (!context.CheckVar(RootListName)) {
                _log.LogError(
                    "Line {l}, column {c}: variable \"{id}\" is not defined.",
                    Token.Line,
                    Token.Column,
                    RootListName);
                return false;
            }
            return Idxs.All(idx => idx.Validate(context)) && NewValueExpr.Validate(context);
        }
    }
}
