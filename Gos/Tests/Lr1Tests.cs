using Compiler;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class Lr1Tests : CompilerTests {
        private ILogger<Lr1> log;
        private ILogger<Lr1Dfa> dfaLog;

        [TestInitialize]
        public void Init() {
            this.log = LoggerFact.CreateLogger<Lr1>();
            this.dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
        }

        [TestMethod]
        public void Parsing() {
            var parser = new Lr1(new Grammar(
                    E,
                    E > (F, eq, F),
                    E > n,
                    F > n + F,
                    F > n),
                this.log,
                this.dfaLog);
            var n1 = new Token(Token.TypeEnum.Number, 1, 1);
            var plus = new Token(Token.TypeEnum.Plus, 1, 2);
            var n2 = new Token(Token.TypeEnum.Number, 1, 3);
            var times = new Token(Token.TypeEnum.Times, 1, 4);
            var n3 = new Token(Token.TypeEnum.Number, 1, 5);
            var eof = new Token(Token.TypeEnum.Eof, 1, 6);
            var _eq = new Token(Token.TypeEnum.Eq, 1, 7);

            Assert.IsTrue(parser.TryParse(
                new[] {
                    n1, _eq, n2, plus, n3, eof  // int = int + int $
                },
                out _));

            parser = new Lr1(new Grammar(
                    E,
                    E > E + T,
                    E > T,
                    T > T * F,
                    T > F,
                    F > (lpar, E, rpar),
                    F > n),
                this.log,
                this.dfaLog);

            Assert.IsTrue(parser.TryParse(
                new[] {
                    n1, plus, n2, times, n3, eof  // int + int * int $
                },
                out _));
            Assert.IsFalse(parser.TryParse(
                new[] {
                    n1, plus, n2, times, eof  // int + int * $
                },
                out _));
        }
    }
}
