using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Compiler.Lexer.Tests {
    internal class TokenCmp : IEqualityComparer<Token> {
        public bool Equals(Token x, Token y) {
            return x.Type == y.Type && (x.Type == Token.TypeEnum.Eof || x.Lexem == y.Lexem);
        }

        public int GetHashCode([DisallowNull] Token obj) {
            return HashCode.Combine(obj.Type, obj.Lexem);
        }
    }
}
