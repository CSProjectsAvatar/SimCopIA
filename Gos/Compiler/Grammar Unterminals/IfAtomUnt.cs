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
        public Token Token { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <if-atom> := "if" <expr> "{" <stat-list> "}"
            
            Condition = (derivation[1] as ExpressionUnt).Ast as Expression;
            Statements = (derivation[3] as StatListUnt).Statements;
            Token = derivation[0] as Token;

            return null;
        }
    }
}
