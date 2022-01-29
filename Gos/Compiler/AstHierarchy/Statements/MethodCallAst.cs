using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class MethodCallAst : Expression, IStatement {
        public Expression Target { get; init; }
        public FunCall Function { get; init; }

        public override bool Validate(Context context) {
            return Target.Validate(context);
        }
    }
}
