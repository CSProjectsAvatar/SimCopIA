using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.Tests {
    [TestClass]
    public class ReLexerTests : LexerTests {
        private Token a => Token.CharFor('a');
        private Token b => Token.CharFor('b');
        private Token c => Token.CharFor('c');
        private Token z => Token.CharFor('z');
        private Token Z => Token.CharFor('Z');
        private Token A => Token.CharFor('A');
        private Token or => Token.Pipe;
        private Token times => Token.Times;
        private Token minus => Token.Minus;
        private Token zero => Token.CharFor('0');
        private Token nine => Token.CharFor('9');
        private Token one => Token.CharFor('1');
        private Token dot => Token.CharFor('.');
        private Token lbrak => Token.LBracket;
        private Token rbrak => Token.RBracket;
        private Token lpar => Token.LPar;
        private Token rpar => Token.RPar;
        private Token quest => Token.Quest;
        private Token plus => Token.Plus;

        private ILogger<ReLexer> _log;

        [TestInitialize]
        public void Init() {
            _log = LoggerFact.CreateLogger<ReLexer>();
        }

        [TestMethod]
        public void Numbers() {
            var lexer = new ReLexer(_log);
            var expected = new[] {
                lbrak, one, minus, nine, rbrak, lbrak, zero, minus, nine, rbrak, times, lpar, dot, lbrak, zero, minus, nine,
                rbrak, plus, rpar, quest
            };
            Assert.IsTrue(lexer.TryTokenize("[1-9][0-9]*(.[0-9]+)?", out var tokens));
            Assert.IsTrue(Enumerable.SequenceEqual(expected, tokens, new TokenCmp()));
        }

        [TestMethod]
        public void Scape() {
            var lexer = new ReLexer(_log);
            var expected = new[] {
                Token.CharFor('*'), Token.CharFor('\\'), a, b, Token.CharFor('-')
            };
            Assert.IsTrue(lexer.TryTokenize(@"\*\\ab\-", out var tokens));
            Assert.IsTrue(Enumerable.SequenceEqual(expected, tokens, new TokenCmp()));
            Assert.IsFalse(lexer.TryTokenize(@"\*a\", out _));  // se espera un meta-caracter
        }
    }

    internal class TokenCmp : IEqualityComparer<Token> {
        public bool Equals(Token x, Token y) {
            return x.Type == y.Type && x.Lexem == y.Lexem;
        }

        public int GetHashCode([DisallowNull] Token obj) {
            return HashCode.Combine(obj.Type, obj.Lexem);
        }
    }
}
