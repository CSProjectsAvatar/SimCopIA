using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class PingUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <ping> := "ping" <atom> "in" <math>
                     | "ping" <atom>
             */
            return new PingAst(Helper.Logger<PingAst>()) {
                Token = derivation[0] as Token,
                Target = (derivation[1] as AtomUnt).Ast as Expression,
                AfterNow = derivation.Count > 2
                    ? (derivation[^1] as MathUnt).Ast as Expression
                    : null
            };
        }
    }
}
