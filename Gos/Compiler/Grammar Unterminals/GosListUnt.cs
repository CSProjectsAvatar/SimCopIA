using Compiler.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class GosListUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <gos-list> := "[" <expr-list> "]"
            //             | "[" "]"
            return new GosListAst {
                Elements = derivation[1] is ExprListUnt el
                    ? el.Exprs
                    : Enumerable.Empty<Expression>(),
                Token = derivation[0] as Token
            };
        }
    }
}
