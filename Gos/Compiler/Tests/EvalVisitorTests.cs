using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Tests {
    [TestClass]
    public class EvalVisitorTests : LangTest {
        [TestMethod]
        public void AccessibleServersServerInsideIf() {
            var tokens = _lex.Tokenize(
                @"
let s1 = new server
let s2 = new server

if true {
    let s3 = new server
    s3.resources = [new resource, new resource]
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            var servs = vis.AccessibleObjs<Server>().ToList();
            Assert.AreEqual(2, servs.Count);
            Assert.IsFalse(servs.Any(s => s.Stats.AvailableResources.Count != 0));
        }

        [TestMethod]
        public void AccessibleServersInListOfLists() {
            var tokens = _lex.Tokenize(
                @"
let l = [[new server, new server], [new server]]
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            var servs = vis.AccessibleObjs<Server>().ToList();
            Assert.AreEqual(3, servs.Count);
        }

        [TestMethod]
        public void AccessibleServersInListReturnedByFunction() {
            var tokens = _lex.Tokenize(
                @"
fun f() {
    let s = new server
    s.resources = [new resource, new resource]

    return [[new server, new server], [new server]]
}
let l = f()
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            var servs = vis.AccessibleObjs<Server>().ToList();
            Assert.AreEqual(3, servs.Count);
            Assert.IsFalse(servs.Any(s => s.Stats.AvailableResources.Count != 0));
        }

        [TestMethod]
        public void AccessibleServersDuplicate() {
            var tokens = _lex.Tokenize(
                @"
let r1 = new resource
let s = new server
s.resources = [r1]
let r = s
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            var servs = vis.AccessibleObjs<Server>().ToList();
            Assert.AreEqual(1, servs.Count);
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
