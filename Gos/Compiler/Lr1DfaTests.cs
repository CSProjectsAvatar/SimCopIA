using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    [TestClass]
    public class Lr1DfaTests : CompilerTests {
        public static readonly UntType S = UntType.S;
        public static readonly UntType E = UntType.E;
        public static readonly UntType T = UntType.T;
        public static readonly UntType F = UntType.F;
        public static readonly UntType X = UntType.X;
        public static readonly UntType Y = UntType.Y;
        public static readonly TokenType n = TokenType.Number;
        public static readonly TokenType plus = TokenType.Plus;
        public static readonly TokenType e = TokenType.Epsilon;
        public static readonly TokenType lpar = Token.TypeEnum.LPar;
        public static readonly TokenType rpar = Token.TypeEnum.RPar;
        public static readonly TokenType eq = Token.TypeEnum.Eq;
        public static readonly TokenType dollar = Token.TypeEnum.Eof;
        private ILogger<Lr1Dfa> _log;

        [TestInitialize]
        public void Init() {
            _log = LoggerFact.CreateLogger<Lr1Dfa>();
        }

        [TestMethod]
        public void EpsilonDerivation() {
            using (var gram = new Grammar(
                    E,
                    E > (X, Y),
                    X > (X, F),
                    X > (F, plus, Y),
                    X > n,
                    F > e,
                    Y > e,
                    T > (T, E),
                    T > (F, Y))) {
                var dfa = new Lr1Dfa();
                dfa.grammar = gram;
                dfa.CalcEpsilonDerivations();

                Assert.IsFalse(dfa.DerivesEpsilon(nameof(FakeE)));
                Assert.IsTrue(dfa.DerivesEpsilon(nameof(FakeY)));
                Assert.IsFalse(dfa.DerivesEpsilon(nameof(FakeX)));
                Assert.IsTrue(dfa.DerivesEpsilon(nameof(FakeF)));
                Assert.IsTrue(dfa.DerivesEpsilon(nameof(FakeT)));
                Assert.IsFalse(dfa.DerivesEpsilon(new[] { n }));
            }
        }

        [TestMethod]
        public void Firsts() {
            using (var gram = new Grammar(
                    E,
                    E > (X, Y),
                    X > (X, F),
                    X > (F, plus, Y),
                    X > n,
                    F > e,
                    Y > e,
                    T > (T, E),
                    T > (F, Y))) {
                var dfa = new Lr1Dfa();
                dfa.grammar = gram;

                dfa.CalcEpsilonDerivations();
                dfa.CalcFirsts();

                var fe = dfa.First(E);
                var fx = dfa.First(X);
                var ff = dfa.First(F);
                var fy = dfa.First(Y);
                var ft = dfa.First(T);

                Assert.AreEqual(3, fe.Count);
                Assert.IsTrue(new[] { 
                        Token.TypeEnum.Number, 
                        Token.TypeEnum.Epsilon,
                        Token.TypeEnum.Plus
                    }
                    .All(t => fe.Contains(t)));

                Assert.AreEqual(3, fx.Count);
                Assert.IsTrue(new[] {
                        Token.TypeEnum.Number,
                        Token.TypeEnum.Epsilon,
                        Token.TypeEnum.Plus
                    }
                    .All(t => fx.Contains(t)));

                Assert.AreEqual(1, ff.Count);
                Assert.IsTrue(new[] {
                        Token.TypeEnum.Epsilon
                    }
                    .All(t => ff.Contains(t)));

                Assert.AreEqual(1, fy.Count);
                Assert.IsTrue(new[] {
                        Token.TypeEnum.Epsilon
                    }
                    .All(t => fy.Contains(t)));

                Assert.AreEqual(3, ft.Count);
                Assert.IsTrue(new[] {
                        Token.TypeEnum.Number,
                        Token.TypeEnum.Epsilon,
                        Token.TypeEnum.Plus
                    }
                    .All(t => ft.Contains(t)));

                var tokenFirst = dfa.First(new[] { n });
                Assert.AreEqual(1, tokenFirst.Count);
                Assert.IsTrue(tokenFirst.Contains(Token.TypeEnum.Number));
            }
        }

        [TestMethod]
        public void InitialClosure() {
            using (var gram = new Grammar(
                    E,
                    E > (F, eq, F),
                    E > n,
                    F > n + F,
                    F > n)) {
                var dfa = new Lr1Dfa();
                dfa.grammar = gram;

                dfa.CalcEpsilonDerivations();
                dfa.CalcFirsts();

                var state = dfa.Closure(Lr1Item.Initial(E));

                Assert.IsTrue(state.Contains(Lr1Item.Initial(E)));
                Assert.IsTrue(state.Contains(new Lr1Item(
                    E > (F, eq, F),
                    dollar,
                    0u
                )));
                Assert.IsTrue(state.Contains(new Lr1Item(
                    E > n,
                    dollar,
                    0u
                )));
                Assert.IsTrue(state.Contains(new Lr1Item(
                    F > n + F,
                    eq,
                    0u
                )));
                Assert.IsTrue(state.Contains(new Lr1Item(
                    F > n,
                    eq,
                    0u
                )));
                Assert.AreEqual(5, state.Count);
            }
        }

        [TestMethod]
        public void RepeatedProductionInClosure() {
            using (var gram = new Grammar(
                    E,
                    E > (F, eq, F),
                    E > n,
                    F > n + F,
                    F > n)) {
                var dfa = new Lr1Dfa();
                dfa.grammar = gram;
                dfa.CalcEpsilonDerivations();
                dfa.CalcFirsts();

                var state = dfa.Closure(new Lr1Item(
                    F > n + F,
                    dollar,
                    2));

                Assert.IsTrue(state.Contains(new Lr1Item(
                    F > n,
                    dollar,
                    0u
                )));
                Assert.IsTrue(state.Contains(new Lr1Item(
                    F > n + F,
                    dollar,
                    0u
                )));
                Assert.IsTrue(state.Contains(new Lr1Item(
                    F > n + F,
                    dollar,
                    2
                )));
                Assert.AreEqual(3, state.Count);
            }
        }

        [TestMethod]
        public void DfaBuilding() {
            using (var gram = new Grammar(
                    E,
                    E > (F, eq, F),
                    E > n,
                    F > n + F,
                    F > n)) {
                var dfa = new Lr1Dfa(gram, _log);
                var actionStates = dfa._action.Keys
                    .Select(k => k.Item1)
                    .Distinct()
                    .Count();
                var gotoStates = dfa._goto.Keys
                    .Select(k => k.Item1)
                    .Distinct()
                    .Count();

                Assert.AreEqual(12, actionStates);  // este es el total d estados
                Assert.AreEqual(4, gotoStates);
            }
        }

        [TestMethod]
        public void SaveToFile() {
            var dfa = new Lr1Dfa();
            dfa._action = new Dictionary<(uint, Token.TypeEnum), (Lr1Dfa.ActionEnum, uint)> {
                [(0, Token.TypeEnum.And)] = (Lr1Dfa.ActionEnum.Reduce, 1),
                [(1, Token.TypeEnum.Or)] = (Lr1Dfa.ActionEnum.Shift, 0),
                [(2, Token.TypeEnum.RightArrow)] = (Lr1Dfa.ActionEnum.Ok, 1)
            };
            dfa._goto = new() {
                [(1, "hello")] = 2,
                [(2, "andy")] = 3
            };
            dfa.SaveToFile("./bla.json");
            using var gram = new Grammar(
                    E,
                    E > (F, eq, F),
                    E > n,
                    F > n + F,
                    F > n);
            dfa = new Lr1Dfa(gram, "./bla.json", _log);

            Assert.AreEqual(Lr1Dfa.ActionEnum.Reduce, dfa._action[(0, Token.TypeEnum.And)].Item1);
            Assert.AreEqual(1u, dfa._action[(0, Token.TypeEnum.And)].Item2);
            Assert.AreEqual(3u, dfa._goto[(2, "andy")]);
        }
    }
}
