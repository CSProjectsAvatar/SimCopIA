using Compiler.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class IsTypeUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <is-type> := <atom> "is" <after-is>
            var afterIs = derivation[^1] as AfterIsUnt;

            return new IsTypeAst(Helper.Logger<IsTypeAst>()) {
                Token = derivation[1] as Token,
                Target = (derivation[0] as AtomUnt).Ast as Expression,
                Not = afterIs.Not,
                Type = afterIs.Type,
                NewVar = afterIs.NewVar
            };
        }
    }
}
