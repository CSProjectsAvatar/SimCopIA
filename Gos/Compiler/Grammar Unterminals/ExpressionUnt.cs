using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class ExpressionUnt : Unterminal
    {
        // <expr> := <math> "<" <math>
        //         | <math> ">" <math>
		//         | <math>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            var leftAst = (derivation[0] as MathUnt).Ast as Expression;

            if(derivation.Count == 1)
                return leftAst;

            var rightAst = (derivation[2] as MathUnt).Ast as Expression;

            return derivation[1] switch{
                Token { Type: Token.TypeEnum.LessThan }  => new LessThanOp(leftAst, rightAst),   
                Token { Type: Token.TypeEnum.GreaterThan }  => new GreaterThanOp(leftAst, rightAst),
                _ => throw new InvalidOperationException() // aki creo q no debemos llegar nunca
            };
        }
    }
}
