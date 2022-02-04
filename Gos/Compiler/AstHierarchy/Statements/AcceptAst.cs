using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class AcceptAst : BehavStatement {
        public AcceptAst(ILogger<AcceptAst> logger) : base(logger) {
        }

        public Expression Request { get; init; }

        public override bool Validate(Context context) {
            if (!base.Validate(context)) {
                return false;
            }
            return Request.Validate(context);
        }
    }
}
