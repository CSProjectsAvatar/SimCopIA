using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class OrderAst : Expression, IStatement {
        private readonly ILogger<OrderAst> _log;

        public OrderAst(ILogger<OrderAst> logger) {
            _log = logger;
        }

        public Expression AfterNow { get; init; }
        public Expression Resources { get; init; }
        public Expression Target { get; init; }

        public override bool Validate(Context context) {
            if (!BehavStatement.Validate(context, _log, Token)) {
                return false;
            }
            return Target.Validate(context) && Resources.Validate(context) && (AfterNow?.Validate(context) ?? true);
        }
    }
}
