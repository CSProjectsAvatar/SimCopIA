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
    public class ServerTests : LangTest {
        [TestMethod]
        public void PropertiesCorrectness() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    print 1
}
behav q {
    print 2
}
let l1 = new layer
l1.behaviors = [p]

let l2 = new layer
l2.behaviors = [q]

let r1 = new resource
let r2 = new resource

let s = new server
s.layers = [l1, l2]
s.resources = [r1, r2]

print s.id
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            var serv = ctx.GetVar("s") as Server;
            serv.HandlePerception(new Request("fulano", "mengano", ReqType.Ping));

            Assert.AreEqual(
                $"serv_1{_endl}" +
                $"1{_endl}" +
                $"2{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void NotInGlobalContext() {
            var tokens = _lex.Tokenize(
                @"
if true {
    let s = new server
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
