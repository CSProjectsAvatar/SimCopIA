using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ElseUnt : Unterminal {
        public IEnumerable<IStatement> Statements { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <else> := "else" "{" <stat-list> "}"
            Statements = (derivation[2] as StatListUnt).Statements;

            return null;
        }
    }
}
