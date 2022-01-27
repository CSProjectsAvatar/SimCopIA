
using DataClassHierarchy;
using System;

namespace Compiler.Lexer.AstHierarchy {
    public class CharAst : BaseRegexAst {
        public char Value { get; init; }

        public override bool Validate(Context context) {
            return true;  // @audit VAMOS A ADMITIR CUALKIER CHAR?
        }

        internal static CharAst FromCharLexeme(string lexem) {
            if (lexem.Length != 1) {
                throw new ArgumentException("Lexem must contain only one character.", nameof(lexem));
            }
            return new CharAst { Value = lexem[0] };
        }
    }
}