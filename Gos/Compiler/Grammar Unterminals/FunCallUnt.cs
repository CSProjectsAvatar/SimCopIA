using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class FunCallUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <func-call> := ID "(" <expr-list> ")"
            var t = derivation[0] as Token;
            string id = t.Lexem;
            var args = (derivation[2] as ExprListUnt).Exprs;

            return new FunCall(){
                Identifier = id,
                Args = args.ToList(),
                Token = t
            };
        }
    }
}