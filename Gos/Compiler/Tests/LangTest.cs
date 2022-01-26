using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.Lexer;

namespace Compiler.Tests {
    [TestClass]
    public class LangTest : BaseTest {
        protected Lexer.Lexer _lex;
        protected Lr1 _parser;
        protected string _dslSuf;
        protected string _endl;

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
";
            _endl = Environment.NewLine;
        }
    }
}
