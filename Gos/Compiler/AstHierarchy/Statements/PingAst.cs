using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class PingAst : BehavStatement {
        public PingAst(ILogger<PingAst> logger) : base(logger) {
        }

        public Expression Target { get; init; }

        /// <summary>
        /// El tiempo que debe transcurrir antes de que el PING arribe.
        /// </summary>
        public Expression AfterNow { get; init; }

        public override bool Validate(Context context) {
            if (!base.Validate(context)) {
                return false;
            }
            return Target.Validate(context) && (AfterNow?.Validate(context) ?? true);
        }
    }
}
