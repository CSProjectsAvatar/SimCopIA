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
    public class ParsingIntegrationTests : CompilerTests {
        #region private fields
        private ILogger<Lr1> log;
        private ILogger<Lr1Dfa> dfaLog;
        private ILogger<EvalVisitor> evalLog;
        private Token[] _dslSuff;

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
        private Token lt => Token.Lt;
        private Token geq => Token.Geq;
        private Token gt => Token.Gt;
        private Token two => Token.NumberFor(2);
        private Token zero => Token.NumberFor(0);
        private Token @if => Token.If;
        private Token @return => Token.Return;
        private Token eqeq => Token.EqEq;
        private Token d => Token.IdFor("d");
        private Token w => Token.IdFor("w");
        private Token @new => Token.New;
        private Token conn => Token.Connection;
        #endregion

        [TestInitialize]
        public void Init() {
            this.log = LoggerFact.CreateLogger<Lr1>();
            this.dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
            this.evalLog = LoggerFact.CreateLogger<EvalVisitor>();
            Helper.LogFact = LoggerFact;
            _dslSuff = new Token[] {
                eof
            };
        }

        [TestMethod]
        public void LetAndPrint() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        let, x, eq, five, endl,  // let x = 5;
                        print, x, endl          // print x;
                    }.Concat(_dslSuff),
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

            Assert.IsTrue(prog.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), this.evalLog, @out);
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
                        print, z, endl                                                 // print z;
                    }.Concat(_dslSuff),
                    out var root));
                AssertIntegration(root, "29", "64", "-6");
            }
        }


        [TestMethod]
        public void UnrecognizedToken() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        let, x, eq, five, plus, Token.IdFor("a"), endl  // let x = 5 + a; 
                    }.Concat(_dslSuff),
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
                        f, lpar, five, rpar, endl           // f(5);
                    }.Concat(_dslSuff),
                    out var root));
                AssertIntegration(root, "5");
            }
        }

        [TestMethod]
        public void If() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        @if, five, lt, eight, lbrace,   // if 5 < 8 {
                            print, one, endl,           //  print 1;
                        rbrace,                         // }
                        @if, five, gt, eight, lbrace,   // if 5 > 8 {
                            print, zero, endl,          //  print 0;
                        rbrace                         // }
                    }.Concat(_dslSuff),
                    out var root));
                AssertIntegration(root, "1");
            }
        }

        // Test para la funcion factorial f n = n * f(n-1)
        [TestMethod]
        public void Factorial() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        fun, f, lpar, n, rpar, lbrace,                                 // fun f(n) {
                            @if, n, eqeq, one, lbrace,                                 //     if n == 1 {
                                @return, one, endl,                                    //         return 1;
                            rbrace,                                                    //     }
                            @if, n, gt, one, lbrace,                                   //     if n > 1 {
                                @return, n, times, f, lpar, n, minus, one, rpar, endl, //        return n * f(n-1);
                            rbrace,                                                    //     }
                        rbrace,                                                        // }
                        print, f, lpar, five, rpar, endl                              // print f(5);
                    }.Concat(_dslSuff),
                    out var root));
                AssertIntegration(root, "120");
            }
        }

        // Test para la funcion fibonacci f n = f(n-1) + f(n-2)
        [TestMethod]
        public void Fibonacci() {
            using (var parser = new Lr1(Grammar, this.log, this.dfaLog)) {
                Assert.IsTrue(parser.TryParse(
                    new[] {
                        fun, f, lpar, n, rpar, lbrace,                                 // fun f(n) {
                            @if, n, eqeq, one, lbrace,                                 //     if n == 1 {
                                @return, one, endl,                                    //         return 1;
                            rbrace,                                                    //     }
                            @if, n, eqeq, two, lbrace,                                 //     if n == 2 {
                                @return, one, endl,                                    //         return 1;
                            rbrace,                                                    //     }
                            @if, n, gt, two, lbrace,                                   //     if n > 2 {
                                @return, f, lpar, n, minus, one, rpar, plus,           //         return f(n-1) + f(n-2);
                                    f, lpar, n, minus, two, rpar, endl, 
                            rbrace,                                                    //     }
                        rbrace,                                                        // }
                        print, f, lpar, five, rpar, endl,                              // print f(5);

                        let, x, eq, eight, endl,                                       // let x = 8;
                        print, f, lpar, x, rpar, endl                                 // print f(x);
                    }.Concat(_dslSuff),
                    out var root));
                AssertIntegration(root, "5", "21");
            }
        }


        
        

    }
}
