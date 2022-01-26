using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Compiler.Lexer.Tests {
    [TestClass]
    public class LexerTests : LexerBaseTests {
        private IEnumerable<(string Regex, Token.TypeEnum Token)> _tknRes => Helper.TokenWithRegexs;
        private ILogger<ReLexer> _logReLex;
        private ILogger<Lr1> _logLr1;
        private ILogger<Lr1Dfa> _logLr1Dfa;
        private ILogger<LexerTests> _log;

        [TestInitialize]
        public void Init() {
            _logReLex = LoggerFact.CreateLogger<ReLexer>();
            _logLr1 = LoggerFact.CreateLogger<Lr1>();
            _logLr1Dfa = LoggerFact.CreateLogger<Lr1Dfa>();
            _log = LoggerFact.CreateLogger<LexerTests>();
            Helper.LogFact = LoggerFact;
        }

        [TestMethod]
        public void HelloWorld() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize("print 07734");  // ahi' dic hello, cuan2 giras la cabeza 90 grados 🙃
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Print, new Token(Token.TypeEnum.Number, 1, 7, "07734"), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void MultipleEndLines() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var endl = Environment.NewLine;
            var tokens = lex.Tokenize($"print 07734{endl}{endl}");  // el 2do \n c debe ignorar
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(  
                tokens,
                new[] {
                    Token.Print, new Token(Token.TypeEnum.Number, 1, 7, "07734"), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void MultipleWhiteSpaces() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize($"print     07734");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Print, new Token(Token.TypeEnum.Number, 1, 7, "07734"), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void NoEndlineAfterBraces() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize(
                @"if a < 3 {
                    print 1
                  }");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.If, Token.IdFor("a"), Token.Lt, Token.NumberFor(3), Token.LBrace,
                        Token.Print, Token.NumberFor(1), Token.Endl,
                    Token.RBrace,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void MultilineStatement() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize(
                @"print 1 \
                        + 3");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Print, Token.NumberFor(1), Token.Plus, Token.NumberFor(3), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void MultipleStatements() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize(
                @"let a = 5 + 3
                  let b = a + 2
                  print a + b");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Let, Token.IdFor("a"), Token.Eq, Token.NumberFor(5), Token.Plus, Token.NumberFor(3), Token.Endl,
                    Token.Let, Token.IdFor("b"), Token.Eq, Token.IdFor("a"), Token.Plus, Token.NumberFor(2), Token.Endl,
                    Token.Print, Token.IdFor("a"), Token.Plus, Token.IdFor("b"), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void NoWhiteSpaces() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize("let a=5+3");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Let, Token.IdFor("a"), Token.Eq, Token.NumberFor(5), Token.Plus, Token.NumberFor(3), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void RestOfLineComment() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize(
                @"let a=5+3#esto suma
                  print 2-1  #y esto imprime 1");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Let, Token.IdFor("a"), Token.Eq, Token.NumberFor(5), Token.Plus, Token.NumberFor(3), Token.Endl,
                    Token.Print, Token.NumberFor(2), Token.Minus, Token.NumberFor(1), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }

        [TestMethod]
        public void EntireLineComment() {
            using var lex = new Lexer(_tknRes, RegexGram, _logReLex, _logLr1, _logLr1Dfa);
            var tokens = lex.Tokenize(
                @"let a=5+3
                  # lo d alla' arriba suma
                  print 2-1");
            _log.LogInformation("{tokens}", tokens.Select(t => t.Lexem));

            Assert.IsTrue(Enumerable.SequenceEqual(
                tokens,
                new[] {
                    Token.Let, Token.IdFor("a"), Token.Eq, Token.NumberFor(5), Token.Plus, Token.NumberFor(3), Token.Endl,
                    Token.Print, Token.NumberFor(2), Token.Minus, Token.NumberFor(1), Token.Endl,
                    Token.Eof
                },
                new TokenCmp()));
        }
    }
}
