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
        private ILogger<Lr1> log;
        private ILogger<Lr1Dfa> dfaLog;
        private ILogger<EvalVisitor> evalLog;
        new private Token let;
        new private Token x;
        new private Token eq;
        new private Token five;
        new private Token plus;
        new private Token three;
        new private Token times;
        new private Token eight;
        new private Token eof;
        new private Token print;
        new private Token endl;
        new private Token lpar;
        new private Token rpar;
        private Token y;
        private Token z;
        private Token minus;

        [TestInitialize]
        public void Init() {
            this.log = LoggerFact.CreateLogger<Lr1>();
            this.dfaLog = LoggerFact.CreateLogger<Lr1Dfa>();
            this.evalLog = LoggerFact.CreateLogger<EvalVisitor>();
            Helper.LogFact = LoggerFact;
            let = Token.Let;
            x = Token.VarX;
            eq = Token.Eq;
            five = Token.Number;
            plus = Token.Plus;
            three = Token.NumberFor(3);
            times = Token.Times;
            eight = Token.NumberFor(8);
            eof = Token.Eof;
            print = Token.Print;
            endl = Token.Endl;
            lpar = Token.LPar;
            rpar = Token.RPar;
            y = Token.VarFor("y");
            z = Token.VarFor("z");
            minus = Token.Minus;
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
    }
}
