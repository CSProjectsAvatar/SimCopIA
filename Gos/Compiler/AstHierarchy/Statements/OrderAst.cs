using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class OrderAst : BehavStatement {
        public OrderAst(ILogger<OrderAst> logger) : base(logger) {
        }

        public Expression AfterNow { get; init; }
        public Expression Resources { get; init; }
        public Expression Target { get; init; }

        public override bool Validate(Context context) {
            if (!base.Validate(context)) {
                return false;
            }
            return Target.Validate(context) && Resources.Validate(context) && (AfterNow?.Validate(context) ?? true);
        }
    }
}
