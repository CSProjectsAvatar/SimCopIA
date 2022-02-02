using Compiler.Grammar_Unterminals;

namespace Compiler {
    internal class GosGrammar : Grammar {
        private static readonly UntType Prog = UntType.Prog;
        private static readonly UntType StatList = new UntType(typeof(StatListUnt));
        private static readonly UntType BlockStat = new UntType(typeof(BlockStUnt));
        private static readonly UntType Stat = new UntType(typeof(StatementUnt));
        private static readonly UntType LetVar = new UntType(typeof(LetVarUnt));
        private static readonly UntType DefFunc = new UntType(typeof(DefFunUnt));
        private static readonly UntType Print = new UntType(typeof(PrintUnt));
        private static readonly UntType IdList = new UntType(typeof(IdListUnt));
        private static readonly UntType Expr = new UntType(typeof(ExpressionUnt));
        private static readonly UntType Math = new UntType(typeof(MathUnt));
        private static readonly GramSymType Math_ = Math;  // para poder aplicar los operadores > y <
        private static readonly UntType Term = new UntType(typeof(TermUnt));
        private static readonly UntType Factor = new UntType(typeof(FactorUnt));
        private static readonly UntType Atom = new UntType(typeof(AtomUnt));
        private static readonly UntType FuncCall = new UntType(typeof(FunCallUnt));
        private static readonly UntType ExprList = new UntType(typeof(ExprListUnt));
        private static readonly UntType If = new UntType(typeof(IfUnt));
        private static readonly UntType IfAtom = new UntType(typeof(IfAtomUnt));
        private static readonly UntType Else = new UntType(typeof(ElseUnt));
        private static readonly UntType ElseIfAtom = new UntType(typeof(ElseIfAtomUnt));
        private static readonly UntType ElseIf = new UntType(typeof(ElseIfUnt));
        private static readonly UntType AfterIf = new UntType(typeof(AfterIfUnt));
        private static readonly UntType Return = new UntType(typeof(ReturnUnt));
        private static readonly UntType Cond = new UntType(typeof(ConditionUnt));
        private static readonly UntType RightConn = new UntType(typeof(RightConnUnt));
        private static readonly UntType GosList = new UntType(typeof(GosListUnt));
        private static readonly UntType ListIdx = new UntType(typeof(ListIdxUnt));
        private static readonly UntType InfLoop = new UntType(typeof(InfLoopUnt));
        private static readonly UntType ForEach = new UntType(typeof(ForEachUnt));
        private static readonly UntType Conjtion = new UntType(typeof(ConjtionUnt));
        private static readonly UntType Disj = new UntType(typeof(DisjUnt));
        private static readonly UntType Behav = new UntType(typeof(BehavUnt));
        private static readonly UntType Init = new UntType(typeof(InitUnt));
        private static readonly UntType PropGet = new UntType(typeof(PropGetUnt));
        private static readonly UntType MethodCall = new UntType(typeof(MethodCallUnt));
        private static readonly UntType IsType = new UntType(typeof(IsTypeUnt));
        private static readonly UntType AfterIs = new UntType(typeof(AfterIsUnt));
        private static readonly UntType IsTypeEnd = new UntType(typeof(IsTypeEndUnt));

        #region terminales
        private static readonly TokenType n = TokenType.Number;
        private static readonly TokenType plus = TokenType.Plus;
        private static readonly TokenType e = TokenType.Epsilon;
        private static readonly TokenType lpar = Token.TypeEnum.LPar;
        private static readonly TokenType rpar = Token.TypeEnum.RPar;
        private static readonly TokenType eq = Token.TypeEnum.Eq;
        private static readonly TokenType dollar = Token.TypeEnum.Eof;
        private static readonly TokenType id = Token.TypeEnum.Id;
        private static readonly TokenType comma = Token.TypeEnum.Comma;
        private static readonly TokenType lbrace = Token.TypeEnum.LBrace;
        private static readonly TokenType rbrace = Token.TypeEnum.RBrace;
        private static readonly TokenType print = Token.TypeEnum.Print;
        private static readonly TokenType lt = Token.TypeEnum.LowerThan;
        private static readonly TokenType gt = Token.TypeEnum.GreaterThan;
        private static readonly TokenType semicolon = Token.TypeEnum.EndOfLine;
        private static readonly TokenType let = Token.TypeEnum.Let;
        private static readonly TokenType fun = Token.TypeEnum.Fun;
        private static readonly TokenType @if = Token.TypeEnum.If;
        private static readonly TokenType @else = Token.TypeEnum.Else;
        private static readonly TokenType elseIf = Token.TypeEnum.ElseIf;
        private static readonly TokenType @return = Token.TypeEnum.Return;
        private static readonly TokenType pipe = Token.TypeEnum.Pipe;
        private static readonly TokenType quest = Token.TypeEnum.Quest;
        private static readonly TokenType rbrak = Token.TypeEnum.RBracket;
        private static readonly TokenType lbrak = Token.TypeEnum.LBracket;
        private static readonly TokenType times = Token.TypeEnum.Times;
        private static readonly TokenType @new = Token.TypeEnum.New;
        private static readonly TokenType rightArrow = Token.TypeEnum.RightArrow;
        private static readonly TokenType forever = Token.TypeEnum.Forever;
        private static readonly TokenType @break = Token.TypeEnum.Break;
        private static readonly TokenType @for = Token.TypeEnum.For;
        private static readonly TokenType @in = Token.TypeEnum.In;
        private static readonly TokenType and = Token.TypeEnum.And;
        private static readonly TokenType or = Token.TypeEnum.Or;
        private static readonly TokenType @bool = Token.TypeEnum.Bool;
        private static readonly TokenType @class = Token.TypeEnum.Class;
        private static readonly TokenType behav = Token.TypeEnum.Behavior;
        private static readonly TokenType init = Token.TypeEnum.InitBehav;
        private static readonly TokenType dot = Token.TypeEnum.Dot;
        private static readonly TokenType respondOrSave = Token.TypeEnum.RespondOrSave;
        private static readonly TokenType process = Token.TypeEnum.Process;
        private static readonly TokenType @is = Token.TypeEnum.Is;
        private static readonly TokenType @not = Token.TypeEnum.Not;

        #endregion
        public GosGrammar() : base(
            Prog,

            Prog > StatList,

            StatList > (Stat, semicolon),
            StatList > (Stat, semicolon, StatList),
            StatList > BlockStat,
            StatList > (BlockStat, StatList),

            BlockStat > If,
            BlockStat > DefFunc,
            BlockStat > InfLoop,
            BlockStat > ForEach,
            BlockStat > Behav,
            BlockStat > Init,

            Behav > (behav, id, lbrace, StatList, rbrace),

            Init > (init, lbrace, StatList, rbrace),

            ForEach > (@for, IdList, @in, Expr, lbrace, StatList, rbrace),

            InfLoop > (forever, lbrace, StatList, rbrace),

            Stat > LetVar,
            Stat > Print,
            Stat > Return,
            Stat > FuncCall,
            Stat > (id, RightConn),
            Stat > (Atom, eq, Expr),  // asignacio'n
            Stat > @break,
            Stat > MethodCall,
            Stat > (respondOrSave, Expr),
            Stat > (process, Expr),

            LetVar > (let, id, eq, Expr),

            DefFunc > (fun, id, lpar, IdList, rpar, lbrace, StatList, rbrace),
            DefFunc > (fun, id, lpar, rpar, lbrace, StatList, rbrace),

            Print > (print, Expr),

            IdList > id,
            IdList > (id, comma, IdList),

            Disj > (Disj, or, Conjtion),
            Disj > Conjtion,

            Conjtion > (Conjtion, and, Cond),
            Conjtion > Cond,

#pragma warning disable CS1718 // Comparison made to same variable
            Cond > (Math_ < Math_),
            Cond > (Math_ > Math_),
            Cond > (Math == Math),
            Cond > Math,
#pragma warning restore CS1718 // Comparison made to same variable

            Expr > Disj,

            ListIdx > Factor[Math],

            GosList > (lbrak, ExprList, rbrak),
            GosList > (lbrak, rbrak),  // lista vaci'a

            RightConn > (rightArrow, IdList),

            Math > Math + Term,
            Math > Math - Term,
            Math > Term,

            Term > Term * Factor,
            Term > Term / Factor,
            Term > Term % Factor,
            Term > Factor,

            Factor > Atom,
            Factor > (lpar, Expr, rpar),

            Atom > n,
            Atom > @bool,
            Atom > id,
            Atom > FuncCall,
            Atom > ListIdx,
            Atom > (@new, @class),
            Atom > GosList,
            Atom > MethodCall,
            Atom > PropGet,
            Atom > IsType,

            IsType > (Atom, @is, AfterIs),

            AfterIs > IsTypeEnd,
            AfterIs > (@not, IsTypeEnd),

            IsTypeEnd > @class,
            IsTypeEnd > (@class, id),

            PropGet > (Factor, dot, id),

            MethodCall > (Factor, dot, FuncCall),

            FuncCall > (id, lpar, ExprList, rpar),
            FuncCall > (id, lpar, rpar),

            ExprList > Expr,
            ExprList > (Expr, comma, ExprList),

        #region if-else
            IfAtom > (@if, Expr, lbrace, StatList, rbrace),

            Else > (@else, lbrace, StatList, rbrace),

            If > IfAtom,
            If > (IfAtom, AfterIf),

            AfterIf > ElseIf,
            AfterIf > Else,
            AfterIf > (ElseIf, Else),

            ElseIfAtom > (elseIf, Expr, lbrace, StatList, rbrace),

            ElseIf > ElseIfAtom,
            ElseIf > (ElseIfAtom, ElseIf),
        #endregion

            Return > (@return, Expr),
            Return > @return) {
        }
    }
}