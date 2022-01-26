using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.AstHierarchy {
    /// <summary>
    /// El nodo correspondiente a la operación *.
    /// </summary>
    class ClosureAst : BaseRegexAst {
        public ClosureAst(BaseRegexAst target) {
            Target = target;
        }

        public BaseRegexAst Target { get; }

        public override bool Validate(Context context) {
            return Target.Validate(context);
        }
    }
}
