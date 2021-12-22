using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    /// <summary>
    /// El nodo correspondiente a la operación |.
    /// </summary>
    public class UnionUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <union> := <union> "|" <concat>
                      | <concat>
             */
            return derivation[0] switch {
                UnionUnt u when derivation[2] is ConcatUnt c => new UnionAst(u.Ast as BaseRegexAst, c.Ast as BaseRegexAst),
                ConcatUnt c => c.Ast,
                _ => throw new ArgumentException("Invalid derivation.", nameof(derivation))
            };
        }
    }
}