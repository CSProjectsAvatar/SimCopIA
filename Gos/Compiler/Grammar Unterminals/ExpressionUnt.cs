using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class ExpressionUnt : Unterminal
    {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            // <expr> := <cond>
	        //         | <math>
            return derivation[0] switch{
                ConditionUnt c => c.Ast,
                MathUnt m => m.Ast,
                _ => throw new ArgumentException("Invalid unterminal.", nameof(derivation))
            };
        }
    }
}
