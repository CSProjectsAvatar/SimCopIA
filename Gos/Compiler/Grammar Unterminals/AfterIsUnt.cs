using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class AfterIsUnt : Unterminal {
        public string Type { get; private set; }
        public string NewVar { get; private set; }
        public bool Not { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <after-is> := <is-type-end>
                         | "not" <is-type-end>
             */
            var isTypeEnd = derivation[^1] as IsTypeEndUnt;
            Type = isTypeEnd.Type;
            NewVar = isTypeEnd.NewVar;
            Not = derivation[0] is Token { Type: Token.TypeEnum.Not };

            return null;
        }
    }
}
