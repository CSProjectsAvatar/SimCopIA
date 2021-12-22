using Compiler;
using Compiler.Grammar_Unterminals;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    public class CompilerTests : BaseTest {
        protected static readonly UntType S = UntType.S;
        protected static readonly UntType E = UntType.E;
        protected static readonly UntType T = UntType.T;
        protected static readonly UntType F = UntType.F;
        protected static readonly UntType X = UntType.X;
        protected static readonly UntType Y = UntType.Y;

        // los verdaderos si'mbolos
        #region no terminales
        protected static readonly UntType Prog = UntType.Prog;
        protected static readonly UntType StatList = new UntType(typeof(StatListUnt));
        protected static readonly UntType BlockStat = new UntType(typeof(BlockStUnt));
        protected static readonly UntType Stat = new UntType(typeof(StatementUnt));
        protected static readonly UntType LetVar = new UntType(typeof(LetVarUnt));
        protected static readonly UntType DefFunc = new UntType(typeof(DefFunUnt));
        protected static readonly UntType Print = new UntType(typeof(PrintUnt));
        protected static readonly UntType ArgList = new UntType(typeof(ArgListUnt));
        protected static readonly UntType Expr = new UntType(typeof(ExpressionUnt));
        protected static readonly UntType Math = new UntType(typeof(MathUnt));
        protected static readonly GramSymType Math_ = Math;  // para poder aplicar los operadores > y <
        protected static readonly UntType Term = new UntType(typeof(TermUnt));
        protected static readonly UntType Factor = new UntType(typeof(FactorUnt));
        protected static readonly UntType Atom = new UntType(typeof(AtomUnt));
        protected static readonly UntType FuncCall = new UntType(typeof(FunCallUnt));
        protected static readonly UntType ExprList = new UntType(typeof(ExprListUnt));
        protected static readonly UntType If = new UntType(typeof(IfUnt));
        protected static readonly UntType Return = new UntType(typeof(ReturnUnt));
        protected static readonly UntType Condition = new UntType(typeof(ConditionUnt));
        #endregion

        #region terminales
        protected static readonly TokenType n = TokenType.Number;
        protected static readonly TokenType plus = TokenType.Plus;
        protected static readonly TokenType e = TokenType.Epsilon;
        protected static readonly TokenType lpar = Token.TypeEnum.LPar;
        protected static readonly TokenType rpar = Token.TypeEnum.RPar;
        protected static readonly TokenType eq = Token.TypeEnum.Eq;
        protected static readonly TokenType dollar = Token.TypeEnum.Eof;
        protected static readonly TokenType id = Token.TypeEnum.Id;
        protected static readonly TokenType comma = Token.TypeEnum.Comma;
        protected static readonly TokenType lbrace = Token.TypeEnum.LBrace;
        protected static readonly TokenType rbrace = Token.TypeEnum.RBrace;
        protected static readonly TokenType print = Token.TypeEnum.Print;
        protected static readonly TokenType lt = Token.TypeEnum.LowerThan;
        protected static readonly TokenType gt = Token.TypeEnum.GreaterThan;
        protected static readonly TokenType semicolon = Token.TypeEnum.EndOfLine;
        protected static readonly TokenType let = Token.TypeEnum.Let;
        protected static readonly TokenType fun = Token.TypeEnum.Fun;
        protected static readonly TokenType @if = Token.TypeEnum.If;
        protected static readonly TokenType @return = Token.TypeEnum.Return;
        protected static readonly TokenType pipe = Token.TypeEnum.Pipe;
        protected static readonly TokenType quest = Token.TypeEnum.Quest;
        protected static readonly TokenType rbrak = Token.TypeEnum.RBracket;
        protected static readonly TokenType lbrak = Token.TypeEnum.LBracket;
        protected static readonly TokenType times = Token.TypeEnum.Times;

        #endregion

        protected static Grammar Grammar => new Grammar(
            Prog,

            Prog > StatList,

            StatList > (Stat, semicolon),
            StatList > (Stat, semicolon, StatList),
            StatList > BlockStat,
            StatList > (BlockStat, StatList),

            BlockStat > If,
            BlockStat > DefFunc,

            Stat > LetVar,
            Stat > Print,
            Stat > Return,
            Stat > FuncCall,

            LetVar > (let, id, eq, Expr),

            DefFunc > (fun, id, lpar, ArgList, rpar, lbrace, StatList, rbrace),

            Print > (print, Expr),

            ArgList > id,
            ArgList > (id, comma, ArgList),

#pragma warning disable CS1718 // Comparison made to same variable
            Condition > (Math_ < Math_),
            Condition > (Math_ > Math_),
            Condition > (Math == Math),
#pragma warning restore CS1718 // Comparison made to same variable

            Expr > Condition,
            Expr > Math,

            Math > Math + Term,
            Math > Math - Term,
            Math > Term,

            Term > Term * Factor,
            Term > Term / Factor,
            Term > Factor,

            Factor > Atom,
            Factor > (lpar, Math, rpar),

            Atom > n,
            Atom > id,
            Atom > FuncCall,

            FuncCall > (id, lpar, ExprList, rpar),

            ExprList > Expr,
            ExprList > (Expr, comma, ExprList),

            If > (@if, Condition, lbrace, StatList, rbrace),

            Return > (@return, Expr)
        );

        #region helpers para tokens
        protected static Token eof => Token.Eof;
        #endregion
    }
}
