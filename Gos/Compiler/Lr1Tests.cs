using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataClassHierarchy;

namespace Compiler {
    [TestClass]
    public class Lr1Tests : CompilerTests {
        private static ILogger<Lr1> _log = LoggerFact.CreateLogger<Lr1>();
        private static ILogger<Lr1Dfa> _dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
        private static ILogger<EvalVisitor> _logEval = LoggerFact.CreateLogger<EvalVisitor>();

        [TestCleanup]
        public void Clean() {
            _parser.Dispose();
        }

        [TestMethod]
        public void Reduce() {
            using (var parser = new Lr1(new Grammar(
                        E,
                        E > (F, eq, F),
                        E > n,
                        F > n + F,
                        F > n),
                    _log,
                    _dfaLog)) {

                var stack = new Stack<(GramSymbol Symbol, uint State)>(new (GramSymbol Symbol, uint State)[] {
                    (Token.Number, 0),
                    (Token.Plus, 3),
                    (Token.Number, 5)
                });  // tamos en el esta2 9

                Assert.IsTrue(parser.TryReduce(4, stack, out var newState));
                Assert.AreEqual(8u, newState);
                Assert.IsInstanceOfType(stack.Peek().Symbol, typeof(FakeF));
                Assert.AreEqual(5u, stack.Peek().State);

                stack = new Stack<(GramSymbol Symbol, uint State)>(new (GramSymbol Symbol, uint State)[] {
                    (Token.Number, 0)
                });  // tamos en el esta2 3

                Assert.IsTrue(parser.TryReduce(2, stack, out newState));
                Assert.AreEqual(1u, newState);
                Assert.IsInstanceOfType(stack.Peek().Symbol, typeof(FakeE));
                Assert.AreEqual(0u, stack.Peek().State);

                stack = new Stack<(GramSymbol Symbol, uint State)>(new (GramSymbol Symbol, uint State)[] {
                    (new FakeF(), 0),
                    (Token.Eq, 2),
                    (new FakeF(), 4)
                });  // tamos en el esta2 6

                Assert.IsTrue(parser.TryReduce(1, stack, out newState));
                Assert.AreEqual(1u, newState);
                Assert.IsInstanceOfType(stack.Peek().Symbol, typeof(FakeE));
                Assert.AreEqual(0u, stack.Peek().State);
            }
        }

        [TestMethod]
        public void Conflicts() {
            using (var parser = new Lr1(new Grammar(
                    E,
                    E > F,
                    F > F + T,
                    F > T,
                    T > n),
                    _log,
                    _dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new Token[] {
                        Token.Number, Token.Plus, Token.Number,
                        Token.Eof
                    },
                    out _));
            }
        }

        [TestMethod]
        public void UnnasignedExpression() {
            Assert.IsFalse(_parser.TryParse(
                new[] {
                    Token.IdFor("a"), Token.Plus, Token.IdFor("b"), Token.Endl,  // a+b;
                    Token.Eof
                },
                out _));
        }

        [TestMethod]
        public void NonRightOperandInSum() {
            Assert.IsFalse(_parser.TryParse(
                new[] {
                    Token.Let, Token.IdFor("a"), Token.Eq, Token.IdFor("b"), Token.Plus, Token.Endl,  // let a = b + ;
                    Token.Eof
                },
                out _));
        }

        [TestMethod]
        public void BraceInNewLine() {
            Assert.IsFalse(_parser.TryParse(
                new[] {
                    Token.Fun, Token.IdFor("f"), Token.LPar, Token.IdFor("param"), Token.RPar, Token.Endl,  // fun f(param);
                    Token.LBrace,                                                                           // {
                    Token.Eof
                },
                out _));
        }

        [TestMethod]
        public void NoClosingBrace() {
            Assert.IsFalse(_parser.TryParse(
                new[] {
                    Token.Fun, Token.IdFor("f"), Token.LPar, Token.IdFor("param"), Token.RPar, Token.LBrace,  // fun f(param) {
                    Token.Return, Token.NumberFor(5), Token.Endl,                                             //    return 5;
                    Token.Eof
                },
                out _));
        }

        [TestMethod]
        public void DivBy0() {
            var ctx = new Context();

            Assert.IsTrue(_parser.TryParse(
                new[] {
                    Token.Let, Token.IdFor("x"), Token.Eq, Token.NumberFor(5), Token.Div, Token.NumberFor(0), Token.Endl,  // let x = 5 / 0;
                    Token.Eof
                },
                out var ast));
            Assert.IsTrue(ast.Validate(ctx));

            var eval = new EvalVisitor(ctx, _logEval, Console.Out);
            Assert.IsFalse(eval.Visit(ast).Item1);
        }

        [TestMethod]
        public void FunctionAlreadyDefined() {
            var ctx = new Context();

            Assert.IsTrue(_parser.TryParse(
                new[] {
                    Token.Fun, Token.IdFor("f"), Token.LPar, Token.IdFor("n"), Token.RPar, Token.LBrace,  // fun f(n) {
                        Token.Return, Token.IdFor("n"), Token.Endl,                                       //    return n;
                    Token.RBrace,                                                                         // }

                    Token.Fun, Token.IdFor("f"), Token.LPar, Token.IdFor("n"), Token.RPar, Token.LBrace,  // fun f(n) {
                        Token.Return, Token.IdFor("n"), Token.Endl,                                       //    return n;
                    Token.RBrace,                                                                         // }
                    Token.Eof
                },
                out var ast));
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void InvalidOperation() {
            var ctx = new Context();

            Assert.IsTrue(_parser.TryParse(
                new[] {
                    Token.Let, Token.IdFor("b"), Token.Eq, Token.NumberFor(5), Token.Lt, Token.NumberFor(8), Token.Endl,  // let b = 5 < 8;
                    Token.Let, Token.IdFor("c"), Token.Eq, Token.NumberFor(5), Token.Plus, Token.IdFor("b"), Token.Endl,
                    Token.Eof
                },
                out var ast));
            Assert.IsTrue(ast.Validate(ctx));

            var eval = new EvalVisitor(new Context(), _logEval, Console.Out);
            Assert.IsFalse(eval.Visit(ast).Item1);
        }

        //[TestMethod]  @todo aki' tienes el test pa cuan2 hagas q pinche el eps en las derivacionesb
        public void EpsilonDerivation() {
            using (var parser = new Lr1(new Grammar(
                        E,
                        E > (F, T),
                        T > (plus, n),
                        T > e,
                        F > n
                    ),
                    _log,
                    _dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new Token[] {
                        Token.Number, Token.Plus, Token.Number,
                        Token.Eof
                    },
                    out _));
                Assert.IsTrue(parser.TryParse(
                    new Token[] {
                        Token.Number,
                        Token.Eof
                    },
                    out _));
            }
        }
    }
}
