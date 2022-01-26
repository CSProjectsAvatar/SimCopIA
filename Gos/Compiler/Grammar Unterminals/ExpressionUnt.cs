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
            // <expr> := <disj>
            //         | "new" CLASS
            //         | <gos-list>
            return derivation[0] switch{
                DisjUnt c => c.Ast,
                Token { Type: Token.TypeEnum.New } _ when derivation[1] is Token { Type: Token.TypeEnum.Class } c => 
                    new ClassAst { ClassName = c.Lexem, Token = c },
                GosListUnt gl => gl.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
