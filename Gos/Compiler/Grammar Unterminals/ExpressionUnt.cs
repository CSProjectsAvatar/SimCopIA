using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.AstHierarchy;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class ExpressionUnt : Unterminal
    {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            // <expr> := <cond>
            //         | <math>
            //         | "simplew"
            //         | "distw"
            return derivation[0] switch{
                ConditionUnt c => c.Ast,
                MathUnt m => m.Ast,
                Token { Type: Token.TypeEnum.SimpleWorker } sw => new SimpleW() {
                    Token = sw
                },
                Token { Type: Token.TypeEnum.DistWorker } dw => new DistW() {
                    Token = dw
                },
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
