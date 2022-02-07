using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy {
    public class PropGetAst : Expression {
        public Expression Target { get; init; }
        public string Property { get; init; }

        public override bool Validate(Context context) {
            return Target.Validate(context);
        }
    }
}
