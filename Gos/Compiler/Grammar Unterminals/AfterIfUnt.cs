using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class AfterIfUnt : Unterminal {
        internal List<Expression> Conditions { get; private set; }
        internal List<IEnumerable<IStatement>> Blocks { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
              <after-if> := <else-if>
                          | <else>
                          | <else-if> <else>
            */
            var conditions = new List<Expression>();
            var blocks = new List<IEnumerable<IStatement>>();

            if (derivation[0] is ElseIfUnt elseIf) {
                foreach (var ei in elseIf.ElseIfs) {
                    conditions.Add(ei.Condition);
                    blocks.Add(ei.Statements);
                }
            }
            if (derivation[^1] is ElseUnt @else) {
                blocks.Add(@else.Statements);
            }
            Conditions = conditions;
            Blocks = blocks;

            return null;
        }
    }
}
