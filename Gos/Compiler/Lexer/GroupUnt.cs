using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class GroupUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <group> := "(" <regex> ")"
            if (derivation[0] is Token { Type: Token.TypeEnum.LPar }
                    && derivation[1] is RegexUnt r
                    && derivation[2] is Token { Type: Token.TypeEnum.RPar }) {
                return r.Ast;
            }
            throw new ArgumentException("Invalid derivation.", nameof(derivation));
        }
    }
}