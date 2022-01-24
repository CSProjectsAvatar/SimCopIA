using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Tests {
    [TestClass]
    public class AssignTests : LangTest {
        [TestMethod]
        public void Correctness() {
            var tokens = _lex.Tokenize(
                @"
                let a = 5
                print a
                a = 3*6/2
                print a" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"5{_endl}9{_endl}", 
                @out.ToString());
        }

        [TestMethod]
        public void VariableNotDefined() {
            var tokens = _lex.Tokenize(
                @"
                let a = 5
                print a
                b = 7
                print a" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void InvalidExpr() {
            var tokens = _lex.Tokenize(
                @"
                let a = 5
                print a
                a = b + c
                print a" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void TypesMismatch() {
            var tokens = _lex.Tokenize(
                @"
                let a = 5
                a = new simplew
                print a" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }
    }
}
