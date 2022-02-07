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
    public class AndOrTests : LangTest {
        [TestMethod]
        public void SimpleAnd() {
            var tokens = _lex.Tokenize(
                @"
fun even(x) {
    return x%2 == 0
}

print 3 == 3 and 4 > 5
print 4+5 == 9 and even(4)
print 3 < 3 and 4 < 5
print 3 < 3 and 4 > 5
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"False{_endl}True{_endl}False{_endl}False{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void AndShortCircuit() {
            var tokens = _lex.Tokenize(
                @"
fun foo() {
    print 0
    return 1==1
}
let a = 5
print a+3 == 9 and foo()
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"False{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void SimpleOr() {
            var tokens = _lex.Tokenize(
                @"
fun even(x) {
    return x%2 == 0
}

print 3 == 3 or 4 > 5
print 4+5 == 9 or even(4)
print 3 < 3 or 4 < 5
print 3 < 3 or 4 > 5
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"True{_endl}True{_endl}True{_endl}False{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void OrShortCircuit() {
            var tokens = _lex.Tokenize(
                @"
fun foo() {
    print 0
    return 1==1
}
let a = 5
print a+3 == 8 or foo()
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"True{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void Mix() {
            var tokens = _lex.Tokenize(
                @"
fun foo() {
    return 1==0
}
let a = 5
print a+3 == 8 and foo() or 1==1 or 3==4 and 5<7
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"True{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void AndCheckBool() {
            var tokens = _lex.Tokenize(
                @"
fun foo() {
    return [1, 3]
}
let a = 5
print foo() and a+3 == 8
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void OrCheckBool() {
            var tokens = _lex.Tokenize(
                @"
fun foo() {
    return [1, 3]
}
let a = 5
print foo() or a+3 == 8
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void Parenthesis() {
            var tokens = _lex.Tokenize(
                @"
fun foo() {
    return 1==0
}
let a = 5
print a+3 == 8 and (foo() or 1==1 or 3==4 and 5<7)
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual(
                $"True{_endl}",
                @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
