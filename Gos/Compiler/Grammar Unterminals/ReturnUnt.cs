using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ReturnUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <return> := "return" <expr>
            return new Return(Helper.Logger<Return>()) {
                Expr = ((Unterminal)derivation[1]).Ast as Expression,
                Token = derivation[0] as Token
            };
        }
    }
}
