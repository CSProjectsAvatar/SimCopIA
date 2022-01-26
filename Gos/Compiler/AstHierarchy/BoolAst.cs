using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy {
    public class BoolAst : Expression {
        public string Value { get; init; }

        public override bool Validate(Context context) {
            return true;
        }
    }
}
