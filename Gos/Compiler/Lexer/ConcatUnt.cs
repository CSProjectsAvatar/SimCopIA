using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class ConcatUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <concat> := <concat> <basic>
                       | <basic>
             */
            return derivation[0] switch {
                ConcatUnt c when derivation[1] is BasicUnt b => new ConcatAst(c.Ast as BaseRegexAst, b.Ast as BaseRegexAst),
                BasicUnt b => b.Ast,
                _ => throw new ArgumentException("Invalid derivation.", nameof(derivation))
            };
        }
    }
}