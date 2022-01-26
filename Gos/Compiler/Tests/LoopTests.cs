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

        [TestMethod]
        public void Foreach() {
            var tokens = _lex.Tokenize(@"
let item = 23

for item in [1, 2, 3] {
    print item
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"1{_endl}2{_endl}3{_endl}", @out.ToString());
        }

        [TestMethod]
        public void ForeachWithIdx() {
            var tokens = _lex.Tokenize(@"
let i = 30

for i, item in [5, 4, 3] {
    print i
    print item
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"1{_endl}5{_endl}2{_endl}4{_endl}3{_endl}3{_endl}", @out.ToString());
        }

        [TestMethod]
        public void NoList() {
            var tokens = _lex.Tokenize(@"
fun f(x) {
    return x+1
}
let l = [f(1), f(2)]

for item in l[2] {
    print item
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void BreakInForeach() {
            var tokens = _lex.Tokenize(@"
for i, item in [5, 4, 3] {
    if i == 3 {
        break
        print 0
    }
    print item
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"5{_endl}4{_endl}", @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }
    }
}
