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

        [TestMethod]
        public void PingTimeNonNumber() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    ping status.my_server in [1, 2]
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

        [TestMethod]
        public void PingTimeNonInteger() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    ping status.my_server in 3.7
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

        [TestMethod]
        public void PingTimeCorrectness() {
            var tokens = _lex.Tokenize(
                @"
behav foo {
    ping status.my_server in 3.00
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

            var serv = new Server("server", _logServ, _logStat);
            serv.Stats.AvailableResources.AddRange(new[] { rs1, rs2 });

            var r1 = new Request("sender", "server", ReqType.DoIt);
            r1.AskingRscs.AddRange(new[] { rs1 });
            var r2 = new Request("sender", "server", ReqType.DoIt);
            r2.AskingRscs.AddRange(new[] { rs2, rs3 });

            serv.Stats.SaveEntry(r1);
            serv.Stats.SaveEntry(r2);
            serv.Stats.AcceptReq(r1);
            serv.Stats.AcceptReq(r2);

            Behavior behav = ctx.GetBehav("foo");
            _ = new Env(_logEnv, _logMicroS);

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
    print [percep.type == DO, percep.type == ASK, percep.type == PING]
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
    } else_if req.type == DO or req.type == PING {
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
        public void RequestsInProcessOutOfRange() {
            var tokens = _lex.Tokenize(
                @"
behav p {
    init {
        ask_call = 1
    }
    if percep.type == ASK {
        if ask_call == 6 {
            respond_or_save percep
        } else {
            respond_or_save done_reqs[1]
        }
        ask_call = ask_call+1
    } else {
        process percep
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
            serv.Stats.SaveEntry(r1);
            var r2 = new Request("fulano", "server", ReqType.Asking);
            serv.Stats.SaveEntry(r2);

            Behavior behavP = ctx.GetBehav("p");
            _ = new Env(_logEnv, _logMicroS);

            #endregion

            behavP.Run(serv.Stats, r1);
            behavP.Run(serv.Stats, r1);
            behavP.Run(serv.Stats, r1);
            behavP.Run(serv.Stats, r1);
            behavP.Run(serv.Stats, r1);
            Assert.ThrowsException<GoSException>(() => behavP.Run(serv.Stats, r1));

            behavP.Run(serv.Stats, r2);
            behavP.Run(serv.Stats, r2);
            behavP.Run(serv.Stats, r2);
            behavP.Run(serv.Stats, r2);
            behavP.Run(serv.Stats, r2);
            Assert.ThrowsException<GoSException>(() => behavP.Run(serv.Stats, r2));
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

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
            MicroService.Services.Clear();
            Resource.Resources.Clear();
        }
    }
}
