using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServersWithLayers;

namespace Compiler.Tests {
    [TestClass]
    public class FunTests : LangTest {
        [TestMethod]
        public void NoArgs() {
            var tokens = _lex.Tokenize(
                @"
fun f() {
    print 1
}
f()" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"1{_endl}", @out.ToString());
        }

        [TestMethod]
        public void NoArgsUsingResult() {
            var tokens = _lex.Tokenize(
                @"
fun f() {
    return 1
}
print f()" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"1{_endl}", @out.ToString());
        }

        [TestMethod]
        public void EmptyReturn() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    if x % 2 == 0 {
        print 0
        return
    }
    print 1
}
f(2)
f(5)" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"0{_endl}1{_endl}", @out.ToString());
        }

        [TestMethod]
        public void OperatingOnEmptyReturn() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    if x % 2 == 0 {
        print 0
        return
    }
    print 1
}
print f(2) + 3" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
