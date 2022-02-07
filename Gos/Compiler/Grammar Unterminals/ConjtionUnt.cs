using Compiler.AstHierarchy.Operands.BooleanOperands;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ConjtionUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <conjtion> := <conjtion> "and" <cond>
                         | <cond>
             */
            return derivation[0] switch {
                ConjtionUnt cu => new ConjtionAst {
                    Left = cu.Ast as Expression,
                    Right = (derivation[2] as ConditionUnt).Ast as Expression,
                    Token = derivation[1] as Token
                },
                ConditionUnt du => du.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
