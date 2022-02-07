using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class AskUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // "ask" <atom> <after-rsrc-req>
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Ask } t when derivation[1] is AtomUnt atomUnt && derivation[2] is AfterRsrcReqUnt after
                    => new AskAst(Helper.Logger<AskAst>()) {
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
