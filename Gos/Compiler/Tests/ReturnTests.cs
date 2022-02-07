using DataClassHierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ServersWithLayers;

namespace Compiler.Tests {
    [TestClass]
    public class ReturnTests : LangTest {
        [TestMethod]
        public void InRootContext() {
            var tokens = _lex.Tokenize(@"
                return 5" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void DeepButNotInFunction() {
            var tokens = _lex.Tokenize(@"
if 3 > 5 {
    if 20 > 30 {
        let a = [3]
        if 3 == 7 {
            return 5
        }
    }
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void NearOfFunctionButNotInside() {
            var tokens = _lex.Tokenize(@"
fun f(x) {
    return x+1
}
if 3 > 5 {
    if 20 > 30 {
        let a = [3]
        if 3 == 7 {
            return 5
        }
    }
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void InsideFunctionWithFunction() {
            var tokens = _lex.Tokenize(@"
fun f(x) {
    let a = 5
    fun g(y) {
        return y+3
    }
    return 3
}" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
