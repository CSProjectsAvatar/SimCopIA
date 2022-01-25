using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }
    }
}
