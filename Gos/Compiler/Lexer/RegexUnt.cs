using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer {
    public class RegexUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <regex> := <union>
            return derivation[0] switch {
                UnionUnt u => u.Ast,
                _ => throw new ArgumentException("Invalid unterminal.", nameof(derivation))
            };
        }
    }
}
