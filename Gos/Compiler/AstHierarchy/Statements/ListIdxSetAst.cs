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
        public Expression NewValueExpr { get; init; }
        public Expression Target { get; internal set; }
        public Expression Idx { get; internal set; }

        public override bool Validate(Context context) {
            return Idx.Validate(context) && NewValueExpr.Validate(context) && Target.Validate(context);
        }
    }
}
