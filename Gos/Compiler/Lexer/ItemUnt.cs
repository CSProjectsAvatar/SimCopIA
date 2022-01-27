using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class ItemUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <item> := <range>
                     | CHAR
            */
            return derivation[0] switch {
                RangeUnt r => r.Ast,
                Token { Type: Token.TypeEnum.Char } t => CharAst.FromCharLexeme(t.Lexem),
                _ => throw new ArgumentException("Too many symbols.", nameof(derivation))
            };
        }
    }
}