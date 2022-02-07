using Compiler;
using Compiler.Grammar_Unterminals;
using Compiler.Tests;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    public class CompilerTests : LangTest {
        protected static readonly UntType S = UntType.S;
        protected static readonly UntType E = UntType.E;
        protected static readonly UntType T = UntType.T;
        protected static readonly UntType F = UntType.F;
        protected static readonly UntType X = UntType.X;
        protected static readonly UntType Y = UntType.Y;

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

        protected static Grammar Grammar => new GosGrammar();

        #region helpers para tokens
        protected static Token eof => Token.Eof;
        #endregion
    }
}
