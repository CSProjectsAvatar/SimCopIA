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
        private static ILogger<Status> _logStat = LoggerFact.CreateLogger<Status>();
        private static ILogger<Env> _logEnv = LoggerFact.CreateLogger<Env>();
        private static ILogger<MicroService> _logMicroS = LoggerFact.CreateLogger<MicroService>();
        private static ILogger<Server> _logServ = LoggerFact.CreateLogger<Server>();

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
            behav.Run(new Status("server", _logStat), null);

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

        [DataTestMethod]
        [DataRow(Helper.StatusVar)]
        [DataRow(Helper.EnvVar)]
        public void AssignMagicVarInInit(string var) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        " + var + @" = [1, 2]
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
            behav.Run(new Status("server", _logStat), null);

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
            behav.Run(new Status("server", _logStat), null);
            behav.Run(new Status("server", _logStat), null);

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

            Assert.ThrowsException<GoSException>(() => behav.Run(new Status("server", _logStat), null));
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
                () => behav.Run(new Status("server", _logStat), new Request("fulano", "mengano", ReqType.Asking)));
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
            behav.Run(new Status("server", _logStat), null);
            behav.Run(new Status("server", _logStat), null);

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

        [DataTestMethod]
        [DataRow("respond_or_save")]
        [DataRow("process")]
        [DataRow("respond")]
        [DataRow("accept")]
        [DataRow("ping")]
        [DataRow("alarm_me in")]
        public void RequestCommandOutsideBehavior(string command) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    print 3
}
let r = 3
" + command + @" r
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsFalse(ast.Validate(new Context()));
        }

        [TestMethod]
        public void PingANonString() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    ping 3
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

            Assert.ThrowsException<GoSException>(() => behavP.Run(null, null));
        }

        [DataTestMethod]
        [DataRow("ping status.my_server", "")]
        [DataRow("alarm_me", "")]
        [DataRow("ask status.my_server", "for []")]
        [DataRow("order status.my_server", "for []")]
        public void ReqTypeTimeNonNumber(string beforeIn, string afterIn) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    " + beforeIn + @" in [1, 2] " + afterIn + @"
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

            Assert.ThrowsException<GoSException>(() => behavP.Run(new Status("me", _logStat), null));
        }

        [DataTestMethod]
        [DataRow("ping status.my_server", "")]
        [DataRow("alarm_me", "")]
        [DataRow("ask status.my_server", "for []")]
        [DataRow("order status.my_server", "for []")]
        public void ReqTypeTimeNonInteger(string beforeIn, string afterIn) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    " + beforeIn + @" in 3.7 " + afterIn + @"
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

            Assert.ThrowsException<GoSException>(() => behavP.Run(new Status("me", _logStat), null));
        }

        [DataTestMethod]
        [DataRow("ping status.my_server", "")]
        [DataRow("alarm_me", "")]
        [DataRow("ask status.my_server", "for []")]
        [DataRow("order status.my_server", "for []")]
        public void ReqTypeTimeCorrectness(string beforeIn, string afterIn) {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    " + beforeIn + @" in 3.00 " + afterIn + @"
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(new Context()));

            var @out = new StringWriter();
            var ctx = new Context();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

            behavP.Run(new Status("me", _logStat), null);
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
            var status = new Status("server", _logStat);
            var r1 = new Request("sender", "server", ReqType.DoIt);
            var r2 = new Request("sender", "server", ReqType.DoIt);
            status.AcceptReq(r1);
            status.AcceptReq(r2);
            new Env(_logEnv, _logMicroS);

            behav.Run(status, null);
            behav.Run(status, null);

            Assert.AreEqual($"2{_endl}" +
                $"{EvalVisitor.GosObjToString(r1)}{_endl}" +
                $"{EvalVisitor.GosObjToString(r2)}{_endl}" +
                $"0{_endl}", @out.ToString());
        }

        [DataTestMethod]
        [DataRow(Helper.StatusVar)]
        [DataRow(Helper.PercepVar)]
        [DataRow(Helper.DoneReqsVar)]
        [DataRow(Helper.EnvVar)]
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
        [DataRow(Helper.EnvVar)]
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
        [DataRow(Helper.EnvVar)]
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
    print [percep.type == ORDER, percep.type == ASK, percep.type == PING]
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            Behavior behav = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

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
            _ = new Env(_logEnv, _logMicroS);

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
            _ = new Env(_logEnv, _logMicroS);

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
            _ = new Env(_logEnv, _logMicroS);
            var r = new Request("fulano", "mengano", ReqType.DoIt);

            #endregion

            behav.Run(null, r);

            Assert.AreEqual(
                    $"{EvalVisitor.GosObjToString(r)}{_endl}" +
                    $"{EvalVisitor.GosObjToString(r)}{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void HiredCorrectness() {
            var tokens = _lex.Tokenize(
                @"
fun i_accept(status, req) {
    return status.can_process
}
behav foo {
    init {
        call = 1
    }
    print call
    call = call + 1
    print status.accepted_reqs

    if percep is not request req {
        return
    }
    if req.type == ASK and i_accept(status, req) {
        respond req
    } else_if req.type == ORDER or req.type == PING {
        accept req
    }
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            var rs1 = new Resource("img1");
            var rs2 = new Resource("img2");
            var rs3 = new Resource("index");

            var serv = new Server("server", _logServ, _logStat);
            serv.Stats.AvailableResources.AddRange(new[] { rs1, rs2 });

            var r1 = new Request("sender", "server", ReqType.Asking);
            r1.AskingRscs.AddRange(new[] { rs1 });
            var r2 = new Request("sender", "server", ReqType.DoIt);
            r2.AskingRscs.AddRange(new[] { rs2, rs3 });
            Behavior behav = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

            #endregion

            behav.Run(serv.Stats, r1);
            behav.Run(serv.Stats, r2);
            behav.Run(serv.Stats, new Observer("fulano"));
            behav.Run(serv.Stats, new Observer("fulano"));

            Assert.AreEqual(
                    $"1{_endl}" +
                    $"[]{_endl}" +
                    $"2{_endl}" +
                    $"[]{_endl}" +
                    $"3{_endl}" +
                    $"[{EvalVisitor.GosObjToString(r2)}]{_endl}" +
                    $"4{_endl}" +
                    $"[{EvalVisitor.GosObjToString(r2)}]{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void AcceptANotDoItRequest() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    init {
        call = 1
    }
    accept percep

    if call == 2 {
        print status.accepted_reqs
    }
    call = call + 1
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
            var serv = new Server("server", _logServ, _logStat);
            var r1 = new Request("fulano", "server", ReqType.DoIt);
            var r2 = new Request("fulano", "server", ReqType.DoIt);
            var r3 = new Request("fulano", "server", ReqType.Asking);

            Behavior behav = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

            #endregion

            behav.Run(serv.Stats, r1);
            behav.Run(serv.Stats, r2);
            Assert.ThrowsException<GoSException>(() => behav.Run(serv.Stats, r3));

            Assert.AreEqual(
                $"[{EvalVisitor.GosObjToString(r1)}, {EvalVisitor.GosObjToString(r2)}]{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void RemovingRequestFromMiddleOfAcceptedQueue() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    init {
        fst_ask = true
    }
    if percep.type == ASK {
        if fst_ask {
            process status.accepted_reqs[2]
            fst_ask = false
        } else {
            print status.accepted_reqs
        }
    } else {
        accept percep
    }
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            #region configuran2
            var serv = new Server("server", _logServ, _logStat);
            var r1 = new Request("fulano", "server", ReqType.DoIt);
            var r2 = new Request("king-kong", "server", ReqType.DoIt);
            var r3 = new Request("godzilla", "server", ReqType.DoIt);
            serv.Stats.SaveEntry(r1);
            var r4 = new Request("pepe-grillo", "server", ReqType.Asking);
            serv.Stats.SaveEntry(r2);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            #endregion

            behavP.Run(serv.Stats, r1);
            behavP.Run(serv.Stats, r2);
            behavP.Run(serv.Stats, r3);
            behavP.Run(serv.Stats, r4);
            behavP.Run(serv.Stats, r4);

            Assert.AreEqual(
                $"[{EvalVisitor.GosObjToString(r1)}, {EvalVisitor.GosObjToString(r3)}]{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void LastOutsideValue() {
            var tokens = _lex.Tokenize(
                @"
let a = 7
behav p {
    print a
}
a = 5
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            behavP.Run(null, null);

            Assert.AreEqual(
                $"5{_endl}", 
                @out.ToString());
        }

        [TestMethod]
        public void AlarmHasNotSender() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    print percep.sender
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            Assert.ThrowsException<GoSException>(() => behavP.Run(null, new Observer("me")));
        }

        [TestMethod]
        public void MessageSender() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    print percep.sender
}
" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            behavP.Run(null, new Request("me", "fulano", ReqType.Ping));
            behavP.Run(null, new Response(1, "you", "fulano", ReqType.Ping, null));

            Assert.AreEqual(
                $"me{_endl}" +
                $"you{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void ParsingFallenLeader() {
            var tokens = _lex.Tokenize(
                @"
behav fallen_leader {
	init {
		init_power = 3
		max_ping = 3
		count_ping = 0
		last_see_leader = 0
	}
	if (percep is request or percep is response) and percep.sender == status.leader {
		count_ping = 0
		last_see_leader = env.time
	}
	let wait_time = pow(2, init_power + count_ping)

	if env.time - last_see_leader > wait_time or env.time - last_see_leader == wait_time {
		if count_ping > max_ping or count_ping == max_ping {  # mucho tiempo sin saber del li'der 
			count_ping = 0
			status.leader = status.my_server  # me pongo de li'der

			for serv in status.neighbors {  # x cada servidor en mi red local (micro-servicio)
				ping serv
			}
			return
		} else {  # construyo pedi2 PING
			let time = rand_int(wait_time/2)
			ping status.leader in time  # envi'o PING
			count_ping = count_ping + 1

			alarm_me in wait_time
		}
	}
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

        [TestMethod]
        public void ResponseRequestType() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    if percep is not response resp {
        return
    }
    print resp.req_type == PING
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            behavP.Run(null, new Request("me", "fulano", ReqType.Ping));
            behavP.Run(null, new Response(1, "you", "fulano", ReqType.Ping, null));

            Assert.AreEqual(
                $"True{_endl}",
                @out.ToString());
        }

        [TestMethod]
        public void AskOrderPingAsExpressions() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    let r1 = ask percep.sender for []
    let r2 = order percep.sender for []
    let r3 = ping percep.sender

    print r1
    print r2
    print r3
}
" + _dslSuf, _builtinCode);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            behavP.Run(new Status("me"), new Request("fulano", "me", ReqType.Ping));

            Assert.AreEqual(
                $"request 2 me --ASK--> fulano (){_endl}" +
                $"request 3 me --DO--> fulano (){_endl}" +
                $"request 4 me --PING--> fulano (){ _endl}",
                @out.ToString());
        }

        [TestMethod]
        public void SaveCorrectness() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    init {
        l = []
    }
    if percep is response resp {
        l.add(resp)
    } else_if percep is request req {
        save l[1] for req
    }
}
let r1 = new resource
let r2 = new resource

let l = new layer
l.behaviors = [p]
l.selector = one_always
" + _dslSuf, _builtinCode);

            Assert.IsTrue(_parser.TryParse(tokens, out var ast));
            Assert.IsTrue(ast.Validate(Context.Global()));

            var @out = new StringWriter();
            var ctx = Context.Global();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);

            Behavior behavP = ctx.GetBehav("p");

            var lay = ctx.GetVar("l") as Layer;
            var r1 = ctx.GetVar("r1") as Resource;
            var r2 = ctx.GetVar("r2") as Resource;
            var serv = new Server("s1", _logServ, _logStat, 5);

            serv.AddLayer(lay);
            serv.SetResources(new[] { r1, r2 });

            var env = new Env(_logEnv, _logMicroS);
            env.AddServerList(new List<Server> { serv });
            
            var req = new Request("mengano", serv.ID, ReqType.DoIt);
            req.AskingRscs.AddRange(new[] { r1, r2 });

            var resp = new Response(
                req.ID,
                "worker_1",
                serv.ID,
                ReqType.DoIt,
                new Dictionary<string, bool> { [r1.Name] = true });

            serv.HandlePerception(resp);
            serv.HandlePerception(req);
        }

        [TestCleanup]
        public void Clean() {
            Dispose();
        }
    }
}
