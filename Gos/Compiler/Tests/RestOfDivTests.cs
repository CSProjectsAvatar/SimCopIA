using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Compiler.Lexer;
using DataClassHierarchy;
using System.IO;
using ServersWithLayers;

namespace Compiler.Tests {
    [TestClass]
    public class RestOfDivTests : LangTest {
        [DataTestMethod]
        [DataRow(5, 3, 2)]
        [DataRow(3, 5, 3)]
        [DataRow(27, 3, 0)]
        [DataRow(6, 3, 0)]
        [DataRow(3.25, 3, 0.25)]
        public void Correctness(double lft, double rgt, double ans) {
            var tokens = _lex.Tokenize($"print {lft} % {rgt} == {ans}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"True{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void ZeroOnRight() {
            var tokens = _lex.Tokenize($"print 3 % 0 == 1" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
