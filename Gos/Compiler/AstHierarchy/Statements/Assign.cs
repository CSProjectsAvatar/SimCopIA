using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class Assign : AstNode, IStatement {
        private readonly ILogger<Assign> _log;

        public string Variable { get; init; }
        public Expression NewValueExpr { get; init; }

        public Assign(ILogger<Assign> logger) {
            _log = logger;
        }

        public override bool Validate(Context context) {
            if (!context.CheckVar(Variable)) {
                _log.LogError(
                    "Line {l}, column {c}: variable {id} is not defined.",
                    Token.Line,
                    Token.Column,
                    Variable);
                return false;
            }
            return NewValueExpr.Validate(context);
        }
    }
}
