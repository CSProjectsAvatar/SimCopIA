using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    [TestClass]
    public class Lr1DfaTests {
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

        [TestMethod]
        public void EpsilonDerivation() {
            var dfa = new Lr1Dfa();
            dfa.grammar = new Grammar(
                E,
                E > (X, Y),
                X > (X, F),
                X > (F, plus, Y),
                X > n,
                F > e,
                Y > e,
                T > (T, E),
                T > (F, Y));
            dfa.CalcEpsilonDerivations();

            Assert.IsFalse(dfa.DerivesEpsilon(nameof(FakeE)));
            Assert.IsTrue(dfa.DerivesEpsilon(nameof(FakeY)));
            Assert.IsFalse(dfa.DerivesEpsilon(nameof(FakeX)));
            Assert.IsTrue(dfa.DerivesEpsilon(nameof(FakeF)));
            Assert.IsTrue(dfa.DerivesEpsilon(nameof(FakeT)));
        }

        [TestMethod]
        public void Firsts() {
            var dfa = new Lr1Dfa();
            dfa.grammar = new Grammar(
                E,
                E > (X, Y),
                X > (X, F),
                X > (F, plus, Y),
                X > n,
                F > e,
                Y > e,
                T > (T, E),
                T > (F, Y));
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
        }

        [TestMethod]
        public void InitialClosure() {
            var dfa = new Lr1Dfa();
            dfa.grammar = new Grammar(
                E,
                E > (F, eq, F),
                E > n,
                F > n + F,
                F > n);
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

        [TestMethod]
        public void RepeatedProductionInClosure() {
            var dfa = new Lr1Dfa();
            dfa.grammar = new Grammar(
                E,
                E > (F, eq, F),
                E > n,
                F > n + F,
                F > n);
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

        [TestMethod]
        public void DfaBuilding() {
            var dfa = new Lr1Dfa(new Grammar(
                    E,
                    E > (F, eq, F),
                    E > n,
                    F > n + F,
                    F > n),
                null);
            var actionStates = dfa.action.Keys
                .Select(k => k.Item1)
                .Distinct()
                .Count();
            var gotoStates = dfa.@goto.Keys
                .Select(k => k.Item1)
                .Distinct()
                .Count();

            Assert.AreEqual(12, actionStates);  // este es el total d estados
            Assert.AreEqual(4, gotoStates);
        }
    }
}
