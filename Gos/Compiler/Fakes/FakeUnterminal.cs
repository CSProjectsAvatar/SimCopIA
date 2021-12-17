using System.Collections.Generic;
using DataClassHierarchy;

namespace Compiler {
    /// <summary>
    /// Se usa para colocar un <see cref="Lr1Item"/> inicial de la forma <code>S' -> .S, $</code>
    /// donde S es el símbolo inicial de la gramática.
    /// </summary>
    internal class FakeUnterminal : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            return null;
        }
    }
}