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
    public class BehaviorTests : LangTest {
        [TestMethod]
        public void NoInit() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    return
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));
        }

        [TestMethod]
        public void InitBlock() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 3
        l = [1, 2, 3]
        c = 3*5 + 2
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));
        }

        [TestMethod]
        public void InitNotFirst() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print 0
    init {
        a = 3
        l = [1, 2, 3]
        c = 3*5 + 2
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void InvalidInitStatement() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        let a = 3
        l = [1, 2, 3]
        c = 3*5 + 2
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void DeepInit() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    if 3 < 5 {
        init {
            let a = 3
            l = [1, 2, 3]
            c = 3*5 + 2
        }
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void UsingInitVars() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 3
        l = [1, 2, 3]
        c = 3*5 + 2
    }
    print a
    print l
    print c
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));
        }

        [TestMethod]
        public void UsingStateAndPercep() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print state
    print percep
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));
        }

        [TestMethod]
        public void StateInInit() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        state = [1, 2]
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void DoubleAssignInInit() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 1
        a = 3
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void OutsideInit() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    return
}
init {
    a = 1
    a = 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void BehavAlreadyDefined() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    return
}
behav foo {
    print 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void NotEmptyReturn() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    return 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void TwoInitBlocks() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 5
    }
    print a

    init {
        l = [1, 2,3]
    }
    print l
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }
    }
}
