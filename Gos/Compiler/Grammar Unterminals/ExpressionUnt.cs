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
            //         | "new" <class>
            return derivation[0] switch{
                ConditionUnt c => c.Ast,
                MathUnt m => m.Ast,
                Token { Type: Token.TypeEnum.New } nw when derivation[1] is ClassUnt cu => 
                    cu.Class switch {
                        ClassUnt.ClassEnum.Simplew => new SimpleW() {
                            Token = nw
                        },
                        ClassUnt.ClassEnum.Distw => new DistW() {
                            Token = nw
                        },
                        _ => throw new NotImplementedException()
                    },
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
