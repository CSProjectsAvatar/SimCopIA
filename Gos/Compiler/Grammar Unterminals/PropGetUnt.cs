using Compiler.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class PropGetUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <prop-get> := <factor> "." ID
            return new PropGetAst {
                Target = (derivation[0] as FactorUnt).Ast as Expression,
                Property = (derivation[2] as Token).Lexem,
                Token = derivation[1] as Token
            };
        }
    }
}
