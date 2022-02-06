using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class AfterRsrcReqUnt : Unterminal {
        public Expression Resources { get; private set; }
        public Expression AfterNow { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <after-rsrc-req> := "in" <expr> "for" <expr>
                               | "for" <expr>
             */
            Resources = (derivation[^1] as ExpressionUnt).Ast as Expression;
            AfterNow = derivation.Count > 2
                ? (derivation[1] as ExpressionUnt).Ast as Expression
                : null;

            return null;
        }
    }
}
