using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class FunCallUnt : Unterminal
    {

        // <func-call> := ID "(" <expr-list> ")"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            string id = (derivation[0] as Token).Lexem;
            var args = (derivation[2] as ExprListUnt).Exprs;

            return new FunCall(){
                Identifier = id,
                Args = args.ToList()
            };
        }
    }
}