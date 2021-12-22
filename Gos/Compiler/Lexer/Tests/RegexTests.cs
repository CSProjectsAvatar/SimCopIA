using Compiler;
using Compiler.Lexer.AstHierarchy;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.Tests {
    [TestClass]
    public class RegexTests : LexerTests {
        private ILogger<Lr1> _log;
        private ILogger<Lr1Dfa> _dfaLog;
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
        private Token lbrak => Token.LBracket;
        private Token rbrak => Token.RBracket;

        [TestInitialize]
        public void Init() {
            _log = LoggerFact.CreateLogger<Lr1>();
            _dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
            Helper.LogFact = LoggerFact;
        }

        [TestMethod]
        public void Concat() {
            using (var parser = new Lr1(RegexGram, _log, _dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        a, b, c, 
                        eof
                    },
                    out var node));
                /*
                      concat1
                       /    \
                   concat2   c
                    /  \
                   a    b           */
                var c1 = node as ConcatAst;
                Assert.IsNotNull(c1);

                var c2 = c1.Left as ConcatAst;
                Assert.IsNotNull(c2);

                var charA = c2.Left as CharAst;
                Assert.IsNotNull(charA);
                Assert.AreEqual('a', charA.Value);

                var charB = c2.Right as CharAst;
                Assert.IsNotNull(charB);
                Assert.AreEqual('b', charB.Value);

                var charC = c1.Right as CharAst;
                Assert.IsNotNull(charC);
                Assert.AreEqual('c', charC.Value);
            }
        }

        [TestMethod]
        public void UnionLeftAsociativity() {
            using (var parser = new Lr1(RegexGram, _log, _dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        a, or, b, or, c,
                        eof
                    },
                    out var node));
                /*
                       union1
                       /    \
                   union2   c
                    /  \
                   a    b           */
                var u1 = node as UnionAst;
                Assert.IsNotNull(u1);

                var u2 = u1.Left as UnionAst;
                Assert.IsNotNull(u2);

                var charA = u2.Left as CharAst;
                Assert.IsNotNull(charA);
                Assert.AreEqual('a', charA.Value);

                var charB = u2.Right as CharAst;
                Assert.IsNotNull(charB);
                Assert.AreEqual('b', charB.Value);

                var charC = u1.Right as CharAst;
                Assert.IsNotNull(charC);
                Assert.AreEqual('c', charC.Value);
            }
        }

        [TestMethod]
        public void PipeTheLessPriority() {
            using (var parser = new Lr1(RegexGram, _log, _dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        a, or, b, times, c,  // a|b*c
                        eof
                    },
                    out var node));
                /*
                       union
                       /    \
                      a    concat
                           /    \
                       closure   c  
                        /
                       b
                */
                var u = node as UnionAst;
                Assert.IsNotNull(u);

                var charA = u.Left as CharAst;
                Assert.IsNotNull(charA);
                Assert.AreEqual('a', charA.Value);

                var concat = u.Right as ConcatAst;
                Assert.IsNotNull(concat);

                var clos = concat.Left as ClosureAst;
                Assert.IsNotNull(clos);

                var charB = clos.Target as CharAst;
                Assert.IsNotNull(charB);
                Assert.AreEqual('b', charB.Value);

                var charC = concat.Right as CharAst;
                Assert.IsNotNull(charC);
                Assert.AreEqual('c', charC.Value);
            }
        }

        [TestMethod]
        public void RangeValidation() {
            using var parser = new Lr1(RegexGram, _log, _dfaLog);
            Assert.IsTrue(parser.TryParse(
                new [] {
                    // [a − zA − Z][a − zA − Z0 − 9]∗
                    lbrak, a, minus, z, A, minus, Z, rbrak, lbrak, a, minus, z, A, minus, Z, zero, minus, nine, rbrak, times,  // [a − zA − Z][a − zA − Z0 − 9]∗
                    eof
                },
                out var ast));
            Assert.IsTrue(ast.Validate(new DataClassHierarchy.Context()));

            Assert.IsTrue(parser.TryParse(
                new[] {
                    // [z − aA − Z][a − zA − Z0 − 9]∗
                    lbrak, z, minus, a, A, minus, Z, rbrak, lbrak, a, minus, z, A, minus, Z, zero, minus, nine, rbrak, times,  // [a − zA − Z][a − zA − Z0 − 9]∗
                    eof
                },
                out ast));
            Assert.IsFalse(ast.Validate(new DataClassHierarchy.Context()));  // z-a no es va'li2

            Assert.IsTrue(parser.TryParse(
                new[] {
                    // [a − zA − Z][a − zA − z0 − 9]∗
                    lbrak, a, minus, z, A, minus, Z, rbrak, lbrak, a, minus, z, A, minus, z, zero, minus, nine, rbrak, times,  // [a − zA − Z][a − zA − Z0 − 9]∗
                    eof
                },
                out ast));
            Assert.IsFalse(ast.Validate(new DataClassHierarchy.Context()));  // A-z no es va'li2
        }
    }
}
