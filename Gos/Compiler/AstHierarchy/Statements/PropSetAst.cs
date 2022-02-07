using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class PropSetAst : AstNode, IStatement {
        public Expression Target { get; init; }
        public string Property { get; init; }
        public Expression NewVal { get; init; }

        public override bool Validate(Context context) {
            return Target.Validate(context) && NewVal.Validate(context);
        }
    }
}
