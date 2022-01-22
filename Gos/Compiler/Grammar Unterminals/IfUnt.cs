using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class IfUnt : Unterminal {
        // <if> := "if" <cond> "{" <stat-list> "}"
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /* <if> := <if-atom>
                     | <if-atom> <after-if> */
            List<Expression> conditions;
            List<IEnumerable<IStatement>> blocks;

            if (derivation.Count > 1) {
                var afterIf = derivation[1] as AfterIfUnt;
                conditions = afterIf.Conditions;
                blocks = afterIf.Blocks;
            } else {
                conditions = new List<Expression>();
                blocks = new List<IEnumerable<IStatement>>();
            }
            var ifAtom = derivation[0] as IfAtomUnt;
            conditions.Insert(0, ifAtom.Condition);
            blocks.Insert(0, ifAtom.Statements);

            return new IfStmt(){
                Conditions = conditions,
                Thens = blocks,
                Token = derivation[0] as Token
            };
        }
    }
}