using Compiler.AstHierarchy;
using DataClassHierarchy;
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
                     | BOOL
                     | ID
                     | <func-call>
                     | <list-idx>
                     | "new" CLASS
                     | <gos-list>
            */
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Number } t => new Number { Value = t.Lexem, Token = t },
                Token { Type: Token.TypeEnum.Bool } t => new BoolAst { Value = t.Lexem, Token = t },
                Token { Type: Token.TypeEnum.Id } t => new Variable { Identifier = t.Lexem, Token = t },
                FunCallUnt or ListIdxUnt or GosListUnt => 
                    (derivation[0] as Unterminal).Ast,
                Token { Type: Token.TypeEnum.New } when derivation[1] is Token { Type: Token.TypeEnum.Class } c =>
                    new ClassAst { ClassName = c.Lexem, Token = c },
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
