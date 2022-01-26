using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class StatementUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <stat> := <let-var>
                     | <print-stat>
                     | <return>
                     | ID <right-conn>
            */
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Id } id when derivation[1] is RightConnUnt rc
                    => new RightConn(Helper.Logger<RightConn>()) {
                        Agents = rc.Ids,
                        LeftAgent = id.Lexem,
                        Token = id
                    },
                LetVarUnt u => u.Ast,
                PrintUnt u => u.Ast,
                ReturnUnt u => u.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
