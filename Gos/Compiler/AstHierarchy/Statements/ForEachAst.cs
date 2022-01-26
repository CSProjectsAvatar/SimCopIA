using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class ForEachAst : AstNode, IStatement {
        public string Item { get; init; }
        public string Index { get; init; }
        public Expression Iterable { get; init; }
        public IEnumerable<IStatement> Code { get; init; }

        public override bool Validate(Context context) {
            context.OpenLoop = true;

            var child = context.CreateChildContext();
            child.DefVariable(Item);
            if (Index != default) {
                child.DefVariable(Index);
            }
            var ans = Code.All(st => st.Validate(child));

            context.OpenLoop = false;

            return ans;
        }
    }
}
