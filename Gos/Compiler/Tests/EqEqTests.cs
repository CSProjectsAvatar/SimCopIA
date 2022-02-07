using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServersWithLayers;

namespace Compiler.Tests {
    [TestClass]
    public class EqEqTests : LangTest {
        [TestMethod]
        public void NumAndBool() {
            var tokens = _lex.Tokenize(
                @"
print 3 == true
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"False{_endl}", @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
