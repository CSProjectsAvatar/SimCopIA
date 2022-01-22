using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ElseIfUnt : Unterminal {
        public IEnumerable<ElseIfAtomUnt> ElseIfs { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <else-if> := <else-if-atom>
            //            | <else-if-atom > <else-if>
            ElseIfs = new[] { derivation[0] as ElseIfAtomUnt };

            if (derivation.Count == 2) {
                ElseIfs = ElseIfs.Concat((derivation[1] as ElseIfUnt).ElseIfs);
            }
            return null;
        }
    }
}
