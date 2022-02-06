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
    public class ResourceTests : LangTest {
        private static ILogger<Status> _logStat = LoggerFact.CreateLogger<Status>();
        private static ILogger<Env> _logEnv = LoggerFact.CreateLogger<Env>();
        private static ILogger<MicroService> _logMicroS = LoggerFact.CreateLogger<MicroService>();
        private static ILogger<Server> _logServ = LoggerFact.CreateLogger<Server>();

        [TestMethod]
        public void PropertiesCorrectness() {
            var tokens = _lex.Tokenize(
                @"
let r = new resource
print r.name
r.required = true
r.time = 3.0

print r.required
print r.time

let r1 = new resource
let r2 = new resource

r.requirements = [r1, r2]
print r.requirements
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Assert.AreEqual(
                $"resrc_1{_endl}" +
                $"True{_endl}" +
                $"3{_endl}" +
                $"[resource resrc_2, resource resrc_3]{ _endl}",
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
