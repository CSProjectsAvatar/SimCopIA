using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class OrderUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // "order" <atom> <after-rsrc-req>
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Order } t when derivation[1] is AtomUnt atomUnt && derivation[2] is AfterRsrcReqUnt after
                    => new OrderAst(Helper.Logger<OrderAst>()) {
                        Token = t,
                        AfterNow = after.AfterNow,
                        Resources = after.Resources,
                        Target = atomUnt.Ast as Expression
                    },
                _ => throw new ArgumentException("Invalid token.", nameof(derivation))
            };
        }
    }
}
