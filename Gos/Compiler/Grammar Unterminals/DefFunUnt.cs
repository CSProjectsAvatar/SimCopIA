using System.Collections.Generic;
using DataClassHierarchy;

namespace Compiler {
    internal class DefFunUnt : Unterminal
    {

        // <def-func> := "fun" ID "(" <arg-list> ")" "{" <stat-list> "}"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            string id = (derivation[1] as Token).Lexem;
            var arg = (derivation[3] as Unterminal).Ast as List<string>;
            var statements = (derivation[6] as Unterminal).Ast as List<IStatement>;

            return new DefFun(){
                Identifier = id,
                Arguments = arg,
                Body = statements
            };
        }
    }
}