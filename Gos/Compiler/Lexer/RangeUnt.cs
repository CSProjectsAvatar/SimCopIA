using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class RangeUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <range> := CHAR "-" CHAR
            if (derivation[0] is Token { Type: Token.TypeEnum.Char } c1
                    && derivation[1] is Token { Type: Token.TypeEnum.Minus }
                    && derivation[2] is Token { Type: Token.TypeEnum.Char } c2) {
                if (c1.Lexem.Length != 1 || c2.Lexem.Length != 1) {
                    throw new ArgumentException("Invalid char-type token.", nameof(derivation));
                }
                return new RangeAst(c1.Lexem[0], c2.Lexem[0], Helper.Logger<RangeAst>());
            }
            throw new ArgumentException("Invalid derivation.", nameof(derivation));
        }
    }
}