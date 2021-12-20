using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    [TestClass]
    /// <summary>
    /// Testea la integración del parseo con la creación de los nodos AST, su validación y ejecución.
    /// </summary>
    public class ParsingIntegrationTests : CompilerTests {  // @audit LOS TESTS D ESTA CLASE FALLAN CUAN2 C CORREN AL MISMO TIEMPO
        #region private fields
        private ILogger<Lr1> log;
        private ILogger<Lr1Dfa> dfaLog;
        private ILogger<EvalVisitor> evalLog;
        private Token let => Token.Let;
        private Token x => Token.VarX;
        private Token eq => Token.Eq;
        private Token five => Token.Number;
        private Token one => Token.NumberFor(1);
        private Token plus => Token.Plus;
        private Token three => Token.NumberFor(3);
        private Token times => Token.Times;
        private Token eight => Token.NumberFor(8);
        private Token eof => Token.Eof;
        private Token print => Token.Print;
        private Token endl => Token.Endl;
        private Token lpar => Token.LPar;
        private Token rpar => Token.RPar;
        private Token y => Token.IdFor("y");
        private Token z => Token.IdFor("z");
        private Token minus => Token.Minus;
        private Token fun => Token.Fun;
        private Token f => Token.IdFor("f");
        private Token lbrace => Token.LBrace;
        private Token rbrace => Token.RBrace;
        private Token n => Token.IdFor("n");
        #endregion

        [TestInitialize]
        public void Init() {
            this.log = LoggerFact.CreateLogger<Lr1>();
            this.dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
            this.evalLog = LoggerFact.CreateLogger<EvalVisitor>();
            Helper.LogFact = LoggerFact;
        }

        [TestMethod]
        public void LetAndPrint() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        let, x, eq, five, endl,  // let x = 5;
                        print, x, endl, eof      // print x;
                    },
                    out var root));
                AssertIntegration(root, "5");
            }
        }

        /// <summary>
        /// Asegura que el parsing haya funcionado bien, valida el programa, lo ejecuta y asegura que imprima las líneas
        /// representadas en <paramref name="expectedOutputLines"/>.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="expectedOutputLines"></param>
        private void AssertIntegration(AstNode root, params string[] expectedOutputLines) {
            var prog = root as ProgramNode;
            Assert.IsNotNull(prog);

            var global = new Context();
            Assert.IsTrue(prog.Validate(global));

            var @out = new StringWriter();
            var vis = new EvalVisitor(global, this.evalLog, @out);
            var (success, _) = vis.Visit(prog);
            Assert.IsTrue(success);

            Assert.AreEqual(
                expectedOutputLines.Aggregate("", (accum, l) => accum + l + Environment.NewLine), 
                @out.ToString());
        }

        [TestMethod]
        public void MathOperations() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        let, x, eq, five, plus, three, times, eight, endl,              // let x = 5 + 3*8;
                        print, x, endl,                                                 // print x;
                        let, y, eq, lpar, five, plus, three, rpar, times, eight, endl,  // let y = (5+3) * 8;
                        print, y, endl,                                                 // print y;
                        let, z, eq, five, minus, three, minus, eight, endl,             // let z = 5 - 3 - 8;
                        print, z, endl,                                                 // print z;
                        eof
                    },
                    out var root));
                AssertIntegration(root, "29", "64", "-6");
            }
        }


        [TestMethod]
        public void UnrecognizedToken() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        let, x, eq, five, plus, Token.IdFor("a"), endl,  // let x = 5 + a; 
                        eof
                    },
                    out var root));
                
                var prog = root as ProgramNode;
                Assert.IsNotNull(prog);

                var global = new Context();
                Assert.IsFalse(prog.Validate(global)); // Comilation Error
            }

        }

        [TestMethod]
        public void Func() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        fun, f, lpar, n, rpar, lbrace,       // fun f(n) {
                            print, n, endl,                  //     print n;
                        rbrace,                              // }
                        f, lpar, five, rpar, endl,           // f(5);
                        eof
                    },
                    out var root));
                AssertIntegration(root, "5");
            }
        }
    

        [TestMethod]
        public void Sucesor() { // Funcion Sucesor
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        fun, f, lpar, n, rpar, lbrace,       // fun f(n) {
                            print, n, plus, one, endl,       //     print n + 1;
                        rbrace,                              // }
                        f, lpar, five, rpar, endl,           // f(5);
                        eof
                    },
                    out var root));
                AssertIntegration(root, "6");
            }
        }
    }
}
