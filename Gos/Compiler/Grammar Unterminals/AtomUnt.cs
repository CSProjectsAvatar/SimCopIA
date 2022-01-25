﻿using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class AtomUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <atom> := NUMBER
                     | <atom-any>
            */
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Number } t => new Number { Value = t.Lexem, Token = t },
                AtomAnyUnt u => u.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
