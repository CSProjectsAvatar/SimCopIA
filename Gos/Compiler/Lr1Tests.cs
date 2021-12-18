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
        private ILogger<Lr1> log;
        private ILogger<Lr1Dfa> dfaLog;
        private ILogger<EvalVisitor> evalLog;

        [TestInitialize]
        public void Init() {
            this.log = LoggerFact.CreateLogger<Lr1>();
            this.dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
            this.evalLog = LoggerFact.CreateLogger<EvalVisitor>();
            Helper.LogFact = LoggerFact;
        }

        [TestMethod]
        public void Reduce() {
            using (var parser = new Lr1(new Grammar(
                        E,
                        E > (F, eq, F),
                        E > n,
                        F > n + F,
                        F > n),
                    this.log,
                    this.dfaLog)) {

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
        public void LetAndPrint() {
            var parser = new Lr1(Grammar, this.log, this.dfaLog);
            var let = Token.Let;
            var x = Token.VarX;
            var eq = Token.Eq;
            var five = Token.Number;
            var eof = Token.Eof;
            var print = Token.Print;
            var endl = Token.Endl;

            Assert.IsTrue(parser.TryParse(
                new[] {
                    let, x, eq, five, endl,
                    print, x, endl, eof
                }, 
                out var root));

            var prog = root as ProgramNode;
            Assert.IsNotNull(prog);
            
            var global = new Context();

            Assert.IsTrue(prog.Validate(global));

            var vis = new EvalVisitor(global, this.evalLog);
            var (success, _) = vis.Visit(prog);
            Assert.IsTrue(success);
        }
    }
}
