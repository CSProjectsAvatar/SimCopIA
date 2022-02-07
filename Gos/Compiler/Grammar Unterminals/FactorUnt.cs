using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class FactorUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <factor> := <atom>
                       | "(" <expr> ")"
            */
            return derivation[0] switch {
                AtomUnt a => a.Ast,
                _ when derivation[1] is ExpressionUnt e => e.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
