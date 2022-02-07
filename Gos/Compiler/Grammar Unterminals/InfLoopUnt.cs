using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class InfLoopUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <inf-loop> := "forever" "{" <stat-list> "}"
            return new InfLoopAst {
                Statements = (derivation[2] as StatListUnt).Statements,
                Token = derivation[0] as Token
            };
        }
    }
}
