using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class DefFunUnt : Unterminal
    {

        // <def-func> := "fun" ID "(" <arg-list> ")" "{" <stat-list> "}"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            string id = (derivation[1] as Token).Lexem;
            var arg = (derivation[3] as ArgListUnt).Args;
            var statements = (derivation[6] as StatListUnt).Statements;

            return new DefFun(){
                Identifier = id,
                Arguments = arg.ToList(),
                Body = statements.ToList()
            };
        }
    }
}