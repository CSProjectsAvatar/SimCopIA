using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer {
    class ReGrammar : Grammar {
        private static UntType Regex = new UntType(typeof(RegexUnt));
        private static UntType Union = new UntType(typeof(UnionUnt));
        private static UntType Concat = new UntType(typeof(ConcatUnt));
        private static UntType Basic = new UntType(typeof(BasicUnt));
        private static UntType Atom = new UntType(typeof(AtomUnt));
        private static UntType Group = new UntType(typeof(GroupUnt));
        private static UntType Set = new UntType(typeof(SetUnt));
        private static UntType ItemList = new UntType(typeof(ItemListUnt));
        private static UntType Item = new UntType(typeof(ItemUnt));
        private static UntType Range = new UntType(typeof(RangeUnt));

        private static TokenType @char = Token.TypeEnum.Char;

        private static readonly TokenType plus = TokenType.Plus;
        private static readonly TokenType lpar = Token.TypeEnum.LPar;
        private static readonly TokenType rpar = Token.TypeEnum.RPar;
        private static readonly TokenType quest = Token.TypeEnum.Quest;
        private static readonly TokenType rbrak = Token.TypeEnum.RBracket;
        private static readonly TokenType lbrak = Token.TypeEnum.LBracket;
        private static readonly TokenType times = Token.TypeEnum.Times;

        public ReGrammar() : base(
            Regex,

            Regex > Union,

            Union > (Union | Concat),
            Union > Concat,

            Concat > (Concat, Basic),
            Concat > Basic,

            Basic > (Atom, times),
            Basic > (Atom, plus),
            Basic > (Atom, quest),
            Basic > Atom,

            Atom > Group,
            Atom > @char,
            Atom > Set,

            Group > (lpar, Regex, rpar),

            Set > (lbrak, ItemList, rbrak),

            ItemList > Item,
            ItemList > (Item, ItemList),

            Item > Range,
            Item > @char,

            Range > @char - @char) { }
    }
}
