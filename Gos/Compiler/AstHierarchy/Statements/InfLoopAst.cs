using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class InfLoopAst : AstNode, IStatement {
        public IEnumerable<IStatement> Statements { get; init; }

        public override bool Validate(Context context) {
            context.OpenLoop = true;

            var child = context.CreateChildContext();
            var ans = Statements.All(st => st.Validate(child));

            context.OpenLoop = false;

            return ans;
        }
    }
}
