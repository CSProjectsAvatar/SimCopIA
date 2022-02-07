using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class BehavUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <behav> := "behav" ID "{" <stat-list> "}"
            return new BehaviorAst(Helper.Logger<BehaviorAst>()) {
                Name = (derivation[1] as Token).Lexem,
                Code = (derivation[^2] as StatListUnt).Statements,
                Token = derivation[0] as Token
            };
        }
    }
}
