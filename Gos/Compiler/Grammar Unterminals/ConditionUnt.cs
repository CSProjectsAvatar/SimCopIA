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
            //         | <math>
            var fstMath = derivation[0] as MathUnt;

            if (derivation.Count == 1) {
                return fstMath.Ast;
            }
            var leftAst = fstMath.Ast as Expression;
            var rightAst = (derivation[2] as MathUnt).Ast as Expression;

            return derivation[1] switch{
                Token { Type: Token.TypeEnum.LowerThan } t  => new LessThanOp(leftAst, rightAst) { Token = t },   
                Token { Type: Token.TypeEnum.GreaterThan } t  => new GreaterThanOp(leftAst, rightAst) { Token = t },
                Token { Type: Token.TypeEnum.EqEq } t  => new EqEqOp(leftAst, rightAst) { Token = t },

                _ => throw new InvalidOperationException()
            };
        }
    }
}
