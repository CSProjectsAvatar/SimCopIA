using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class AtomUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <atom> := <group> 
                     | CHAR
                     | <set>
             */
            return derivation[0] switch {
                GroupUnt g => g.Ast,
                Token { Type: Token.TypeEnum.Char } t => CharAst.FromCharLexeme(t.Lexem),
                SetUnt s => s.Ast,
                _ => throw new ArgumentException("Invalid unterminal.", nameof(derivation))
            };
        }
    }
}