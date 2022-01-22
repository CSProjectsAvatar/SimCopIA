using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class IfAtomUnt : Unterminal {
        public Expression Condition { get; private set; }
        public IEnumerable<IStatement> Statements { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <if-atom> := "if" <cond> "{" <stat-list> "}"
            
            Condition = (derivation[1] as ConditionUnt).Ast as Expression;
            Statements = (derivation[3] as StatListUnt).Statements;

            return null;
        }
    }
}
