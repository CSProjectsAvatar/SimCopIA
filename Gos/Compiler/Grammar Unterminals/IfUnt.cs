using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class IfUnt : Unterminal
    {
        // <if> := "if" <cond> "{" <stat-list> "}"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            return new IfStmt(){
                Condition = (derivation[1] as ConditionUnt).Ast as Expression,
                Then = (derivation[3] as StatListUnt).Statements.ToList()
            };
        }
    }
}