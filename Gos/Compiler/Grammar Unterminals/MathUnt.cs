using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class MathUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <math> := <math> "+" <term>
                     | <math> "-" <term>
                     | <term>
             */
            return derivation[0] switch {
                MathUnt m when derivation[1] is Token { Type: Token.TypeEnum.Plus } t
                    => new AddOp(
                            left: m.Ast as Expression,
                            right: (derivation[2] as TermUnt).Ast as Expression) {
                        Token = t 
                    },
                MathUnt m when derivation[1] is Token { Type: Token.TypeEnum.Minus } t
                    => new SubOp(
                            left: m.Ast as Expression,
                            right: (derivation[2] as TermUnt).Ast as Expression) {
                        Token = t 
                    },
                TermUnt t => t.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
