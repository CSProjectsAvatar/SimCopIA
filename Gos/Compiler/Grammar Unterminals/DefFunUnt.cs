using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class DefFunUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <def-func> := "fun" ID "(" <arg-list> ")" "{" <stat-list> "}"
            var t = derivation[1] as Token;
            string id = t.Lexem;
            var arg = (derivation[3] as IdListUnt).Ids;
            var statements = (derivation[6] as StatListUnt).Statements;

            return new DefFun(Helper.Logger<DefFun>()){
                Identifier = id,
                Arguments = arg.ToList(),
                Body = statements.ToList(),
                Token = t
            };
        }
    }
}