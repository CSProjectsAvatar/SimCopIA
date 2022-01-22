using Compiler.Lexer;
using Core;
using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Tests {
    [TestClass]
    public class IfElseTests : BaseTest {
        private Lexer.Lexer _lex;
        private Lr1 _parser;
        private string _dslSuf;

        [TestInitialize]
        public void Init() {
            var logReLex = LoggerFact.CreateLogger<ReLexer>();
            var logLr1 = LoggerFact.CreateLogger<Lr1>();
            var logLr1Dfa = LoggerFact.CreateLogger<Lr1Dfa>();
            var logLex = LoggerFact.CreateLogger<Lexer.Lexer>();

            Helper.LogFact = LoggerFact;

            _lex = new Lexer.Lexer(Helper.TokenWithRegexs, new ReGrammar(), logReLex, logLr1, logLr1Dfa, logLex);
            _parser = new Lr1(new GosGrammar(), logLr1, logLr1Dfa);
            _dslSuf = @"
                let d = new distw
                let w = new simplew
                d -> w";
        }

        [TestCleanup]
        public void Clean() {
            _lex.Dispose();
            _parser.Dispose();
        }

        [TestMethod]
        public void SimpleElseWorking() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 0
                } else {
                    print 1
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"1{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void ElseWithElseIfWorking() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 0
                } else_if 3 == 5 {
                    print 1
                } else {
                    print 2
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"2{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void ElseWithTwoElseIfs() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 0
                } else_if 3 == 5 {
                    print 1
                } else_if 3 > 5 {
                    print 2
                } else {
                    print 3
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"3{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void ElseIfTrue() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 0
                } else_if 3 == 5 {
                    print 1
                } else_if 3 < 5 {
                    print 2
                } else {
                    print 3
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual($"2{Environment.NewLine}", @out.ToString());
        }

        [TestMethod]
        public void NoElse() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 0
                } else_if 3 > 5 {
                    print 1
                } else_if 3 == 5 {
                    print 2
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsTrue(ast.Validate(ctx));

            var @out = new StringWriter();
            var vis = new EvalVisitor(ctx, LoggerFact.CreateLogger<EvalVisitor>(), @out);
            var (success, _) = vis.Visit(ast);

            Assert.IsTrue(success);
            Assert.AreEqual("", @out.ToString());
        }

        [TestMethod]
        public void IfConditionNotValid() {
            var tokens = _lex.Tokenize(@"
                if a == b {
                    print 0
                } else_if 3 > 5 {
                    print 1
                } else_if 3 == 5 {
                    print 2
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void ElseIfConditionNotValid() {
            var tokens = _lex.Tokenize(@"
                if 3 < 5 {
                    print 0
                } else_if 3 == 5 {
                    print 1
                } else_if a == b {
                    print 2
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void IfCodeNotValid() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    let c = a == b
                } else_if 3 == 5 {
                    print 1
                } else_if 3 > 5 {
                    print 2
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void ElseIfCodeNotValid() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 1
                } else_if 3 == 5 {
                    print 2
                } else_if 3 > 5 {
                    let c = a == b
                } else {
                    print 4
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }

        [TestMethod]
        public void ElseCodeNotValid() {
            var tokens = _lex.Tokenize(@"
                if 3 > 5 {
                    print 1
                } else_if 3 == 5 {
                    print 2
                } else_if 3 > 5 {
                    print 4
                } else {
                    let c = a == b
                }" + _dslSuf);
            Assert.IsTrue(_parser.TryParse(tokens, out var ast));

            var ctx = new Context();
            Assert.IsFalse(ast.Validate(ctx));
        }
    }
}
