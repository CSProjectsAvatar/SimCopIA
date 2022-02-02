using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy {
    public class IsTypeAst : Expression {
        private ILogger<IsTypeAst> _log;

        public IsTypeAst(ILogger<IsTypeAst> logger) {
            _log = logger;
        }

        public Expression Target { get; init; }
        public bool Not { get; init; }
        public string Type { get; init; }
        public string NewVar { get; init; }

        public override bool Validate(Context context) {
            if (!Target.Validate(context)) {
                return false;
            }
            if (NewVar != default && !context.DefVariable(NewVar)) {
                _log.LogError(
                    Helper.LogPref + "variable '{id}' already defined.",
                    Token.Line,
                    Token.Column,
                    NewVar);
                return false;
            }
            return true;
        }
    }
}
