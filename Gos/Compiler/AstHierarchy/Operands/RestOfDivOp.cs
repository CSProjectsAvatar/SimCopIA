using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Operands {
    public class RestOfDivOp : NumOp {
        public RestOfDivOp(Expression left, Expression right) : base(left, right) {
        }

        public override (bool, object) TryCompute(object left, object right) {  // is pending for removal
            throw new InvalidOperationException();
        }
    }
}
