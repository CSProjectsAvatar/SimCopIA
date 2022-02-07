using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class PingAst : Expression, IStatement {
        private readonly ILogger<PingAst> _log;

        public PingAst(ILogger<PingAst> logger) {
            _log = logger;
        }

        public Expression Target { get; init; }

        /// <summary>
        /// El tiempo que debe transcurrir antes de que el PING arribe.
        /// </summary>
        public Expression AfterNow { get; init; }

        public override bool Validate(Context context) {
            if (!BehavStatement.Validate(context, _log, Token)) {
                return false;
            }
            return Target.Validate(context) && (AfterNow?.Validate(context) ?? true);
        }
    }
}
