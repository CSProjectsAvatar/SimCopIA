using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy {
    public class ListIdxGetAst : Expression {
        public Expression Index { get; init; }
        public Expression Left { get; init; }

        public override bool Validate(Context context) {
            return Left.Validate(context) && Index.Validate(context);
        }
    }
}
