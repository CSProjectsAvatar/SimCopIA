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
    public class BoolTests : LangTest {
        [TestMethod]
        public void InVariable() {
            var tokens = _lex.Tokenize(@"
let a = true
let b = false
if a {
    print 0
}
if b {
    print 1
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"0{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void InReturn() {
            var tokens = _lex.Tokenize(@"
fun even(x) {
    if x%2 == 0 {
        return true
    }
    return false
}
if even(2) {
    print 0
}
if even(3) {
    print 1
}
if even(24) {
    print 2
}
if even(5) {
    print 3
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"0{Environment.NewLine}2{_endl}", @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }
    }
}
