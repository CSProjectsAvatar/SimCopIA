using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ToIdxUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <to-idx> := ID
                       | <func-call>
                       | <gos-list>
                       | <list-idx>
             */
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Id } id
                    => new Variable(Helper.Logger<Variable>()) { Token = id, Identifier = id.Lexem },
                Unterminal u => u.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
