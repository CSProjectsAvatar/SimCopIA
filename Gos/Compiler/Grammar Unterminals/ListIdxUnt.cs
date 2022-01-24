using Compiler.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ListIdxUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <list-idx> := <to-idx> "[" <math> "]"
            var toIdx = derivation[0] as ToIdxUnt;

            return new ListIdxGetAst {
                Token = toIdx.Ast.Token,
                Left = toIdx.Ast as Expression,
                Index = (derivation[2] as MathUnt).Ast as Expression
            };
        }
    }
}
