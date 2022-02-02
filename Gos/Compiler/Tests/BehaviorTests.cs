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
                () => behav.Run(new Status("server"), new Request("fulano", "mengano", ReqType.Asking)));
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

        [TestMethod]
        public void RespondOrSaveOutside() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print 3
}
let r = 3
respond_or_save r
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void ProcessOutside() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print 3
}
let r = 3
process r
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void ProcessRemovesAcceptedRequests() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print status.accepted_reqs.length

    for req in status.accepted_reqs {
        if status.can_process == false {
            break
        }
        print req
        process req
    }
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
            var status = new Status("server");
            var r1 = new Request("sender", "server", ReqType.Asking);
            var r2 = new Request("sender", "server", ReqType.Asking);
            status.AcceptReq(r1);
            status.AcceptReq(r2);
            new Env();

            behav.Run(status, null);
            behav.Run(status, null);

            Assert.AreEqual($"2{_endl}" +
                $"{EvalVisitor.GosObjToString(r1)}{_endl}" +
                $"{EvalVisitor.GosObjToString(r2)}{_endl}" +
                $"0{_endl}", @out.ToString());
        }

        [TestMethod]
        public void RespondOrSaveRemovesFromDoneReqs() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        call = 1
    }
    print call
    print true

    for req in done_reqs {
        print req
        respond_or_save req
    }
    print false

    for req in status.accepted_reqs {
        if status.can_process == false {
            break
        }
        print req
        process req
    }
    call = call + 1
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            var rs1 = new Resource("img1");
            var rs2 = new Resource("img2");
            var rs3 = new Resource("index");

            var serv = new Server("server");
            serv.Stats.AvailableResources.AddRange(new[] { rs1, rs2 });

            var r1 = new Request("sender", "server", ReqType.Asking);
            r1.AskingRscs.AddRange(new[] { rs1 });
            var r2 = new Request("sender", "server", ReqType.Asking);
            r2.AskingRscs.AddRange(new[] { rs2, rs3 });

            serv.Stats.AcceptReq(r1);
            serv.Stats.AcceptReq(r2);

            Behavior behav = ctx.GetBehav("foo");
            _ = new Env();

            #endregion

            behav.Run(serv.Stats, null);
            behav.Run(serv.Stats, null);
            behav.Run(serv.Stats, null);

            Assert.AreEqual(
                $"1{_endl}" +
                $"True{_endl}" +
                $"False{_endl}" +
                $"{EvalVisitor.GosObjToString(r1)}{_endl}" +
                $"{EvalVisitor.GosObjToString(r2)}{_endl}" +
                $"2{_endl}" +
                $"True{_endl}" +
                $"{EvalVisitor.GosObjToString(r1)}{_endl}" +
                $"{EvalVisitor.GosObjToString(r2)}{_endl}" +
                $"False{_endl}" +
                $"3{_endl}" +
                $"True{_endl}" +
                $"False{_endl}", @out.ToString());
        }

        [DataTestMethod]
        [DataRow(Helper.StatusVar)]
        [DataRow(Helper.PercepVar)]
        [DataRow(Helper.DoneReqsVar)]
        public void MagicVarCantBeAssignedInInit(string var) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        " + var + @" = 3
    }
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [DataTestMethod]
        [DataRow(Helper.StatusVar)]
        [DataRow(Helper.PercepVar)]
        [DataRow(Helper.DoneReqsVar)]
        public void MagicVarCantBeAssignedInMainCode(string var) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    " + var + @" = 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [DataTestMethod]
        [DataRow(Helper.StatusVar)]
        [DataRow(Helper.PercepVar)]
        [DataRow(Helper.DoneReqsVar)]
        public void MagicVarCanBeAssignedOutside(string var) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print true
}
let " + var + @" = 3
" + var + " = 5" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));
        }

        [TestMethod]
        public void RequestTypeProperty() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print [percep.type == DO, percep.type == ASK, percep.type == PING]
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            Behavior behav = ctx.GetBehav("foo");
            _ = new Env();

            #endregion

            behav.Run(null, new Request("fulano", "mengano", ReqType.Asking));
            behav.Run(null, new Request("fulano", "mengano", ReqType.DoIt));
            behav.Run(null, new Request("fulano", "mengano", ReqType.Ping));

            Assert.AreEqual(
                $"[False, True, False]{_endl}" +
                $"[True, False, False]{_endl}" +
                $"[False, False, True]{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void PercepIs() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print [percep is request, percep is response, percep is alarm]
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            Behavior behav = ctx.GetBehav("foo");
            _ = new Env();

            #endregion

            behav.Run(null, new Observer("fulano"));
            behav.Run(null, new Request("fulano", "mengano", ReqType.DoIt));
            behav.Run(null, new Response(0, "fulano", "mengano", ReqType.DoIt, null));

            Assert.AreEqual(
                $"[False, False, True]{_endl}" +
                $"[True, False, False]{_endl}" +
                $"[False, True, False]{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void PercepIsNot() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print [percep is not request, percep is not response, percep is not alarm]
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            Behavior behav = ctx.GetBehav("foo");
            _ = new Env();

            #endregion

            behav.Run(null, new Observer("fulano"));
            behav.Run(null, new Request("fulano", "mengano", ReqType.DoIt));
            behav.Run(null, new Response(0, "fulano", "mengano", ReqType.DoIt, null));

            Assert.AreEqual(
                $"[True, True, False]{_endl}" +
                $"[False, True, True]{_endl}" +
                $"[True, False, True]{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void PercepIsAlreadyDefined() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    let a = 5
    print [percep is not request a, percep is not response, percep is not alarm]
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(Context.Global()));
        }

        [TestMethod]
        public void PercepIsWithVarInIf() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    if percep is request req {
        print req
    }
    print req
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            Behavior behav = ctx.GetBehav("foo");
            _ = new Env();
            var r = new Request("fulano", "mengano", ReqType.DoIt);

            #endregion

            behav.Run(null, r);

            Assert.AreEqual(
                    $"{EvalVisitor.GosObjToString(r)}{_endl}" +
                    $"{EvalVisitor.GosObjToString(r)}{_endl}",
                @out.ToString());
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
            MicroService.Services.Clear();
        }
    }
}
