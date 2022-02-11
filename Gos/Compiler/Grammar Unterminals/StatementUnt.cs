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
                     | "respond_or_save" <expr>
                     | "process" <expr>
                     | "respond" <expr>
                     | "accept" <expr>
                     | <ping>
                     | "alarm_me" "in" <expr>
                     | <ask>
                     | <order>
                     | "save" <atom> "for" <atom>
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
                Token { Type: Token.TypeEnum.RespondOrSave } t when derivation[1] is ExpressionUnt exprUnt
                    => new RespondOrSaveAst(Helper.Logger<RespondOrSaveAst>()) {
                        Token = t,
                        Request = exprUnt.Ast as Expression
                    },
                Token { Type: Token.TypeEnum.Process } t when derivation[1] is ExpressionUnt exprUnt
                    => new ProcessAst(Helper.Logger<ProcessAst>()) {
                        Token = t,
                        Request = exprUnt.Ast as Expression
                    },
                Token { Type: Token.TypeEnum.Respond } t when derivation[1] is ExpressionUnt exprUnt
                    => new RespondAst(Helper.Logger<RespondAst>()) {
                        Token = t,
                        Request = exprUnt.Ast as Expression
                    },
                Token { Type: Token.TypeEnum.Accept } t when derivation[1] is ExpressionUnt exprUnt
                    => new AcceptAst(Helper.Logger<AcceptAst>()) {
                        Token = t,
                        Request = exprUnt.Ast as Expression
                    },
                PingUnt p => p.Ast,
                Token { Type: Token.TypeEnum.AlarmMe } t when derivation[^1] is ExpressionUnt exprUnt
                    => new AlarmMeAst(Helper.Logger<AlarmMeAst>()) {
                        Token = t,
                        AfterNow = exprUnt.Ast as Expression
                    },
                AskUnt u => u.Ast,
                OrderUnt u => u.Ast,
                Token { Type: Token.TypeEnum.Save } t when derivation[1] is AtomUnt respAtom && derivation[^1] is AtomUnt reqAtom
                    => new SaveAst(Helper.Logger<SaveAst>()) {
                        Token = t,
                        Response = respAtom.Ast as Expression,
                        Request = reqAtom.Ast as Expression
                    },
                _ => throw new ArgumentException("Invalid symbol.", nameof(derivation))
            };
        }
    }
}
