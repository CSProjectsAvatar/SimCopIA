using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class IfUnt : Unterminal
    {
        // <if> := "if" <expr> "{" <stat-list> "}"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            return new IfStmt(){
                Condition = (derivation[1] as ExpressionUnt).Ast as Expression,
                Then = (derivation[3] as StatListUnt).Ast
            };
        }
    }
}