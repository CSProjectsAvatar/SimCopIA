using Compiler.AstHierarchy;
using Compiler.AstHierarchy.Statements;
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
                     | <func-call>
                     | ID <right-conn>
                     | <atom> "=" <expr>
                     | "break"
                     | <method-call>
            */
            return derivation[0] switch {
                Token { Type: Token.TypeEnum.Id } id when derivation[1] is RightConnUnt rc
                    => new RightConn(Helper.Logger<RightConn>()) {
                        Agents = rc.Ids,
                        LeftAgent = id.Lexem,
                        Token = id
                    },
                AtomUnt a when derivation[2] is ExpressionUnt e
                    => new AssignAst(Helper.Logger<AssignAst>()) {
                        Left = a.Ast as Expression,
                        NewVal = e.Ast as Expression,
                        Token = derivation[1] as Token
                    },
                LetVarUnt u => u.Ast,
                PrintUnt u => u.Ast,
                ReturnUnt u => u.Ast,
                FunCallUnt u => u.Ast,
                Token { Type: Token.TypeEnum.Break } b => new BreakAst(Helper.Logger<BreakAst>()) {
                    Token = b
                },
                MethodCallUnt u => u.Ast,
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
