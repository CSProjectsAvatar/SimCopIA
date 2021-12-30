using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class RightConnUnt : Unterminal {
        public IEnumerable<string> Ids { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <right-conn> := "->" <id-list>
            if (derivation[0] is Token { Type: Token.TypeEnum.RightArrow }
                    && derivation[1] is IdListUnt idList) {
                Ids = idList.Ids;
            } else {
                throw new ArgumentException("Invalid symbols.", nameof(derivation));
            }
            return null;
        }
    }
}
