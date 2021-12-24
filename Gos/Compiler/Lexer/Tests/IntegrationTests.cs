using Compiler.Lexer.AstHierarchy;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.Tests {
    [TestClass]
    public class IntegrationTests : LexerTests {
        private ILogger<ReLexer> _logReLexer;
        private ILogger<Lr1> _log;
        private ILogger<Lr1Dfa> _logDfa;

        [TestInitialize]
        public void Init() {
            Helper.LogFact = LoggerFact;
            _logReLexer = LoggerFact.CreateLogger<ReLexer>();
            _log = LoggerFact.CreateLogger<Lr1>();
            _logDfa = LoggerFact.CreateLogger<Lr1Dfa>();
        }

        [DataTestMethod]
        [DataRow("let", "let", true)]
        [DataRow("let", "lets", false)]
        [DataRow("let", "le", false)]
        [DataRow("[0-9]+(.[0-9]+)?", "1.23", true)]
        [DataRow("[0-9]+(.[0-9]+)?", "1", true)]
        [DataRow("[0-9]+(.[0-9]+)?", "000", true)]
        [DataRow("[0-9]+(.[0-9]+)?", "0.5", true)]
        [DataRow("[0-9]+(.[0-9]+)?", ".5", false)]
        [DataRow("[0-9]+(.[0-9]+)?", "23.", false)]
        [DataRow("_?[a-zA-Z][_a-zA-Z0-9]*", "a", true)]
        [DataRow("_?[a-zA-Z][_a-zA-Z0-9]*", "_Andy_123_", true)]
        [DataRow("_?[a-zA-Z][_a-zA-Z0-9]*", "anDy123", true)]
        [DataRow("_?[a-zA-Z][_a-zA-Z0-9]*", "a1CASI", true)]
        [DataRow("_?[a-zA-Z][_a-zA-Z0-9]*", "0Esteno", false)]
        [DataRow("_?[a-zA-Z][_a-zA-Z0-9]*", "_0EsteTampoco", false)]
        public void RecognizeToken(string regex, string word, bool match) {
            Assert.IsTrue(new ReLexer(_logReLexer).TryTokenize(regex, out var tokens));

            using var parser = new Lr1(RegexGram, _log, _logDfa);
            Assert.IsTrue(parser.TryParse(tokens.Append(Token.Eof), out var reAst));
            Assert.IsTrue(reAst.Validate(new DataClassHierarchy.Context()));

            var nfa = new NfaBuilderVisitor().Visit(reAst);
            var dfa = new ConverterToDFA(nfa).ToDFA();

            if (TryWalk(dfa, word, out var lastState)) {  // pu2 consumir toa la palabra
                Assert.AreEqual(match, dfa.FinalStates.Contains(lastState));  // el u'ltimo estado es un estado final?
            } else {
                Assert.IsFalse(match);
            }
        }

        private bool TryWalk(DFA dfa, string word, out uint lastState) {
            lastState = dfa.Initial;
            foreach (var c in word) {
                if (!dfa.Transitions.TryGetValue((lastState, c), out lastState)) {
                    return false;
                }
            }
            return true;
        }
    }
}
