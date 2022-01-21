using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    public class ConditionUnt : Unterminal {

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <cond> := <math> "<" <math>
		    //         | <math> ">" <math>
            //         | <math> "==" <math>
            var leftAst = (derivation[0] as MathUnt).Ast as Expression;
            var rightAst = (derivation[2] as MathUnt).Ast as Expression;

            return derivation[1] switch{
                Token { Type: Token.TypeEnum.LowerThan }  => new LessThanOp(leftAst, rightAst),   
                Token { Type: Token.TypeEnum.GreaterThan }  => new GreaterThanOp(leftAst, rightAst),
                Token { Type: Token.TypeEnum.EqEq }  => new EqEqOp(leftAst, rightAst),

                _ => throw new InvalidOperationException()
            };
        }
    }
}
