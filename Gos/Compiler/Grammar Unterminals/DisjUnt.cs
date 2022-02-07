using Compiler.AstHierarchy.Operands.BooleanOperands;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Grammar_Unterminals {
    internal class DisjUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <disj> := <disj> "or" <conjtion>
                     | <conjtion>
             */
            return derivation[0] switch {
                DisjUnt du => new DisjAst {
                    Left = du.Ast as Expression,
                    Right = (derivation[2] as ConjtionUnt).Ast as Expression,
                    Token = derivation[1] as Token
                },
                ConjtionUnt c => c.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}