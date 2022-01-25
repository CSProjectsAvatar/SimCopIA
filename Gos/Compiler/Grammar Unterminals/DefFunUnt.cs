using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class DefFunUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <def-func> := "fun" ID "(" <arg-list> ")" "{" <stat-list> "}"
            //             | "fun" ID "(" ")" "{" <stat-list> "}"
            var t = derivation[1] as Token;
            string id = t.Lexem;
            var arg = derivation[3] is IdListUnt idList
                ? idList.Ids
                : Enumerable.Empty<string>();
            var statements = (derivation[^2] as StatListUnt).Statements;

            return new DefFun(Helper.Logger<DefFun>()){
                Identifier = id,
                Arguments = arg.ToList(),
                Body = statements.ToList(),
                Token = t
            };
        }
    }
}