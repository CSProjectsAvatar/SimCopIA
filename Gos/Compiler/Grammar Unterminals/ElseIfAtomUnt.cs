using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ElseIfAtomUnt : Unterminal {
        public Expression Condition { get; private set; }
        public IEnumerable<IStatement> Statements { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <else-if-atom> := "else if" <cond> "{" <stat-list> "}"
            Condition = (derivation[1] as ConditionUnt).Ast as Expression;
            Statements = (derivation[^2] as StatListUnt).Statements;

            return null;
        }
    }
}
