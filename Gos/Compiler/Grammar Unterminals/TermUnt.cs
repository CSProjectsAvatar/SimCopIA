﻿using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class TermUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <term> := <term> "*" <factor>
                     | <term> "/" <factor>
                     | <factor>
            */
            return derivation[0] switch {
                TermUnt t when derivation[1] is Token { Type: Token.TypeEnum.Times }
                    => new MultOp(
                        left: t.Ast as Expression,
                        right: (derivation[2] as FactorUnt).Ast as Expression
                    ),
                TermUnt t when derivation[1] is Token { Type: Token.TypeEnum.Div }
                    => new DivOp(
                        left: t.Ast as Expression,
                        right: (derivation[2] as FactorUnt).Ast as Expression,
                        logger: Helper.Logger<DivOp>()
                    ),
                FactorUnt f => f.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
