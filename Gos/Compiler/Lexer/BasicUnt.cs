using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;

namespace Compiler.Lexer {
    public class BasicUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <basic> := <atom> "*" 
                      | <atom> "+" 
                      | <atom> "?" 
                      | <atom>
            */
            if (derivation[0] is not AtomUnt atom) {
                throw new ArgumentException($"{nameof(AtomUnt)} expected.", nameof(derivation));
            }
            if (derivation.Count == 1) {
                return atom.Ast;
            }
            return derivation[1] switch {
                Token { Type: Token.TypeEnum.Times } => new ClosureAst(atom.Ast as BaseRegexAst),
                Token { Type: Token.TypeEnum.Plus } => new PositClosureAst(atom.Ast as BaseRegexAst),
                Token { Type: Token.TypeEnum.Quest } => new ZeroOnceAst(atom.Ast as BaseRegexAst),
                _ => throw new ArgumentException("Invalid derivation.", nameof(derivation))
            };
        }
    }
}