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
    public class LayerTests : LangTest {
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
let l = new layer
l.behaviors = [p, q]
l.selector = one_always
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            var layer = ctx.GetVar("l") as Layer;
            var serv = new Server("me");
            serv.AddLayer(layer);

            for (int i = 0; i < 20; i++) {
                serv.HandlePerception(new Request("fulano", "me", ReqType.Ping));
            }
            Assert.AreEqual(
                string.Join(
                    _endl, 
                    Enumerable.Range(1, 20).Select(_ => 1))
                + _endl,
                @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
            MicroService.Services.Clear();
            Resource.Dispose();
            Message.Dispose();
            Helper.Dispose();
        }
    }
}
