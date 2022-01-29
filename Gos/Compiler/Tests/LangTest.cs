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
        private static ILogger<ReLexer> logReLex = LoggerFact.CreateLogger<ReLexer>();
        private static ILogger<Lr1> logLr1 = LoggerFact.CreateLogger<Lr1>();
        private static ILogger<Lr1Dfa> logLr1Dfa = LoggerFact.CreateLogger<Lr1Dfa>();
        private static ILogger<Lexer.Lexer> logLex = LoggerFact.CreateLogger<Lexer.Lexer>();
        protected static Lexer.Lexer _lex = new Lexer.Lexer(Helper.TokenWithRegexs, new ReGrammar(), logReLex, logLr1, logLr1Dfa, logLex);
        protected static Lr1 _parser = new Lr1(new GosGrammar(), "./lr1-dfa.json", logLr1, logLr1Dfa);
        protected string _dslSuf;
        protected string _endl;

        [TestInitialize]
        public void Init() {
            Helper.LogFact = LoggerFact;
            _dslSuf = @"
";
            _endl = Environment.NewLine;
        }
    }
}
