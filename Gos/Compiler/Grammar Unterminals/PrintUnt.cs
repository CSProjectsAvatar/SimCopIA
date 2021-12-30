using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class PrintUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <print-stat> := "print" <expr>
            return new Print {
                Expr = (derivation[1] as ExpressionUnt).Ast as Expression,
                Token = derivation[0] as Token
            };
        }
    }
}
