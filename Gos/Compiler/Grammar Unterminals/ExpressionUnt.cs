using System;
using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class ExpressionUnt : Unterminal
    {
        // <expr> := <math> "<" <math>
        //         | <math> ">" <math>
		//         | <math>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            var leftAst = (derivation[0] as MathUnt).Ast;

            if(derivation.Count == 1)
                return leftAst;

            var rightAst = (derivation[2] as MathUnt).Ast;

            return derivation[1] switch{
                Token { Type: TokenType.LessThan }  => new LessThanOp(leftAst, rightAst),
                
                Token { Type: TokenType.GreaterThan }  => new GreaterThanOp(leftAst, rightAst),
               
                _ => throw new InvalidOperationException() // @audit aki creo q no debemos llegar nunca
            };

        }
    }
}