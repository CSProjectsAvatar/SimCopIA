using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class IsTypeEndUnt : Unterminal {
        public string Type { get; private set; }
        public string NewVar { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <is-type-end> := CLASS
                            | CLASS ID
             */
            if (derivation[0] is not Token { Type: Token.TypeEnum.Class } c) {
                throw new ArgumentException("Invalid token.", nameof(derivation));
            }
            Type = c.Lexem;
            NewVar = derivation[^1] is Token { Type: Token.TypeEnum.Id } t
                ? t.Lexem
                : default;
            
            return null;
        }
    }
}
