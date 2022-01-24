using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Tests {
    [TestClass]
    public class ListTests : LangTest {
        [TestMethod]
        public void Creation() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x + 1
}

let l = [1+3, f(5), 0, 3*2/6]
print l" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"[4, 6, 0, 1]{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void NotSameType() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return new simplew
}

let l = [1+3, 0, 3*2/6, f(5)]
print l" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void ErrorInElement() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x + 1
}

let l = [1+3, f(5), 0, a*2/6]
print l" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void Indexing() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x + 1
}

let l = [1+3, f(5), 0, 3*2/6]
print l[1]
print l[24/12]
print l[f(2)]
print l[4]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"4{_endl}6{_endl}0{_endl}1{_endl}", @out.ToString());
        }

        [TestMethod]
        public void IdxingOnListOfLists() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x + 1
}

let l = [[f(5)], [[1+3], [0], [3]], [0], [3*2/6]]
print l[2][1][1]
print l[24/12-1][1]
print l[f(2)]
print l[4]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"4{_endl}6{_endl}[0]{_endl}[1]{_endl}", @out.ToString());
        }

        [TestMethod]
        public void IdxingOnFunResult() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return [x, x+1, x+2]
}

print f(3)[2]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"4{_endl}", @out.ToString());
        }

        [TestMethod]
        public void IdxAssignment() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [1, 2, 3]

l[f(1)] = 5
print l" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"[1, 5, 3]{_endl}", @out.ToString());
        }

        [TestMethod]
        public void ChangeElementInListOfLists() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [\
    [1, 2, 3], \
    [4, 5, 6], \
    [7, 8, 9]]

l[f(1)][3] = 13
print l" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"[[1, 2, 3], [4, 5, 13], [7, 8, 9]]{_endl}", @out.ToString());
        }

        [TestMethod]
        public void TypeOfNewElementMismatch() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [1, 2, 3]

l[f(1)] = new simplew
print l" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(4)]
        [DataRow(5)]
        public void IdxOutOfRangeInGet(int idx) {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [1, 2, 3]
print l[" + idx + "]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(4)]
        [DataRow(5)]
        public void IdxOutOfRangeInSet(int idx) {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [1, 2, 3]
l[" + idx + "] = 10" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void NegativeIdxInSet() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [1, 2, 3]
let a = 0-5
l[a] = 10" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void NegativeIdxInGet() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return x+1
}
let l = [1, 2, 3]
let a = 0-5
print l[a]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void IdxNotANumber() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return new simplew
}
let l = [1, 2, 3]
print l[f(5)]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void IdxNotAnInteger() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return 3.000000007
}
let l = [1, 2, 3]
print l[f(5)]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void IdxAlmostAFloat() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return 3.0000000000
}
let l = [1, 2, 3]
print l[f(5)]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"3{_endl}", @out.ToString());
        }

        [TestMethod]
        public void IndexingWithOperands() {
            var tokens = _lex.Tokenize(
                @"
fun f(x) {
    return 3.0000000000
}
let l = [1, 2, 3]
print 3 + 2*5 + l[f(5)] - 23" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"-7{_endl}", @out.ToString());
        }

        [TestMethod]
        public void IndexingInNonList() {
            var tokens = _lex.Tokenize(
                @"
let a = 3
print a[1]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void IndexingSetInNonList() {
            var tokens = _lex.Tokenize(
                @"
let a = 3
a[1] = 5" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void IndexingInNestedNonList() {
            var tokens = _lex.Tokenize(
                @"
let a = [1, 2, 3]
print a[2][1]" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(new Context(), LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void IndexingSetInNestedNonList() {
            var tokens = _lex.Tokenize(
                @"
let a = [1, 2, 3]
a[2][1] = 12" + _dslSuf);
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
