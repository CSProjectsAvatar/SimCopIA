using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class AlarmMeAst : BehavStatement {
        public AlarmMeAst(ILogger<AlarmMeAst> logger) : base(logger) {
        }

        /// <summary>
        /// Análogo a <see cref="PingAst.AfterNow"/>. Este no puede ser null.
        /// </summary>
        public Expression AfterNow { get; init; }

        public override bool Validate(Context context) {
            if (!base.Validate(context)) {
                return false;
            }
            return AfterNow.Validate(context);
        }
    }
}
