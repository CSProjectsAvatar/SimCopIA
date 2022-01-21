using DataClassHierarchy;
using System.Collections.Generic;

namespace Compiler {
    internal class FakeS : FakeUnterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            return (derivation[0] as Unterminal).Ast;
        }
    }
}