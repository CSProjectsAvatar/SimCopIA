using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class SetUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <set> := "[" <item-list> "]"
            if (derivation[0] is Token { Type: Token.TypeEnum.LBracket }
                    && derivation[1] is ItemListUnt il
                    && derivation[2] is Token { Type: Token.TypeEnum.RBracket }) {
                return new SetAst(il.Items);
            }
            throw new ArgumentException("Invalid derivation.", nameof(derivation));
        }
    }
}