using Compiler;
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
        protected static readonly TokenType n = TokenType.Number;
        protected static readonly TokenType plus = TokenType.Plus;
        protected static readonly TokenType e = TokenType.Epsilon;
        protected static readonly TokenType lpar = Token.TypeEnum.LPar;
        protected static readonly TokenType rpar = Token.TypeEnum.RPar;
        protected static readonly TokenType eq = Token.TypeEnum.Eq;
        protected static readonly TokenType dollar = Token.TypeEnum.Eof;
    }
}
