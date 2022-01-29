using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class MethodCallUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <method-call> := <factor> "." <func-call>
            return new MethodCallAst {
                Target = (derivation[0] as FactorUnt).Ast as Expression,
                Function = (derivation[^1] as FunCallUnt).Ast as FunCall,
                Token = derivation[1] as Token
            };
        }
    }
}
