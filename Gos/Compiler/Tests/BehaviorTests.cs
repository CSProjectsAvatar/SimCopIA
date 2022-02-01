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
using Core;

namespace Compiler.Tests {
    [TestClass]
    public class BehaviorTests : LangTest {
        [TestMethod]
        public void PercepNotDefinedInInit() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        b = status.otra_cosa
        a = percep.algo
    }
    return
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

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

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behav = ctx.GetBehav("foo");
            behav.Run(new Status("server"), null);

            Assert.AreEqual($"3{_endl}[1, 2, 3]{Environment.NewLine}17{_endl}", @out.ToString());
        }

        [TestMethod]
        public void UsingStateAndPercep() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print status
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
        status = [1, 2]
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

        [TestMethod]
        public void StatusVars() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print done_reqs
    print status.accepted_reqs
    print status.can_process
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behav = ctx.GetBehav("foo");
            behav.Run(new Status("server"), null);

            Assert.AreEqual($"[]{_endl}[]{Environment.NewLine}True{_endl}", @out.ToString());
        }

        [TestMethod]
        public void VarPersistence() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 3
    }
    print a
    a = a + 1
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behav = ctx.GetBehav("foo");
            behav.Run(new Status("server"), null);
            behav.Run(new Status("server"), null);

            Assert.AreEqual($"3{_endl}4{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void OperatingWithStatus() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 3
    }
    let b = a + status
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behav = ctx.GetBehav("foo");

            Assert.ThrowsException<GoSException>(() => behav.Run(new Status("server"), null));
        }

        [TestMethod]
        public void OperatingWithPercep() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 3
    }
    let b = a + percep
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behav = ctx.GetBehav("foo");

            Assert.ThrowsException<GoSException>(
                () => behav.Run(new Status("server"), new Request("fulano", "mengano", RequestType.AskSomething)));
        }

        [TestMethod]
        public void LocalVar() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        a = 3
    }
    let c = 3
    print c
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behav = ctx.GetBehav("foo");
            behav.Run(new Status("server"), null);
            behav.Run(new Status("server"), null);

            Assert.AreEqual($"3{_endl}3{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void SetDoneReqsHeap() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    __done_reqs_heap = 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void GetDoneReqsHeap() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    let a = __done_reqs_heap + 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void LetDoneReqsHeap() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    let __done_reqs_heap = 3
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
