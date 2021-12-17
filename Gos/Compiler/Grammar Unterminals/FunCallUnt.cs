using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class DefFunUnt : Unterminal
    {

        // <func-call> := ID "(" <expr-list> ")"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            string id = (derivation[0] as Token).Lexem;
            var arg = (derivation[2] as expr).Args;
            var statements = (derivation[6] as StatListUnt).Statements;

            return new DefFun(){
                Identifier = id,
                Arguments = arg.ToList(),
                Body = statements
            };
        }
    }
}