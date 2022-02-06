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
            /* <expr> := <disj>
                       | <ping>
                       | <ask>
                       | <order>
             */
            return derivation[0] switch{
                DisjUnt or PingUnt or AskUnt or OrderUnt => (derivation[0] as Unterminal).Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
