using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy {
    public class GosListAst : Expression {
        public IEnumerable<Expression> Elements { get; init; }

        public override bool Validate(Context context) {
            return Elements.All(expr => expr.Validate(context));
        }
    }
}
