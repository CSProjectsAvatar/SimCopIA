using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Compiler.Tests {
    [TestClass]
    public class LoopTests : LangTest {
        [TestMethod]
        public void Forever() {
            var tokens = _lex.Tokenize(@"
let a = 1
forever {
    if 1 == 1 {
        if a == 5 {
            break
        }
    }
    print a
    a = a + 1
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"1{_endl}2{_endl}3{_endl}4{_endl}", @out.ToString());
        }

        [TestMethod]
        public void BreakNotInForever() {
            var tokens = _lex.Tokenize(@"
let a = 1
forever {
    print a
    a = a + 1
}
if 1 == 1 {
    if a == 5 {
        break
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void LetInsideForever() {
            var tokens = _lex.Tokenize(@"
let a = 1
let i = 0
forever {
    if i == 3 {
        let a = 3
        print a
        break
    }
    let a = 5
    print a
    i = i + 1
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"5{_endl}5{_endl}5{_endl}3{_endl}", @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }
    }
}
