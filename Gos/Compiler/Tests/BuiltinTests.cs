using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataClassHierarchy;
using System.IO;
using ServersWithLayers;

namespace Compiler.Tests {
    [TestClass]
    public class BuiltinTests : LangTest {
        [TestMethod]
        public void LineAndColumnOfTokensRemainsUnaffected() {
            var tokens = _lex.Tokenize(
                @"
let a =
" + _dslSuf, _builtinCode);
            Assert.IsFalse(_parser.TryParse(tokens, out _));
        }

        [TestMethod]
        public void Pow() {
            var tokens = _lex.Tokenize(
                @"
print pow(2, 3)
print pow(3, 1 + 2)
print pow(5, 0)
print pow(24, 1)
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Assert.AreEqual(
                $"8{_endl}" +
                $"27{_endl}" +
                $"1{_endl}" +
                $"24{_endl}",
                @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
