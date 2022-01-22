using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer {
    public class Lexer : IDisposable {
        private readonly DFA _dfa;
        private readonly ILogger<Lexer> _log;

        public Lexer(
                IEnumerable<(string Regex, Token.TypeEnum Token)> tokenRegexs,
                Grammar regexGrammar,
                ILogger<ReLexer> reLexerLog,
                ILogger<Lr1> lr1Logger,
                ILogger<Lr1Dfa> lr1DfaLogger,
                ILogger<Lexer> log = null) {
            _log = log;

            var reLex = new ReLexer(reLexerLog);
            using var reParser = new Lr1(regexGrammar, lr1Logger, lr1DfaLogger);
            var reVis = new NfaBuilderVisitor();
            NFA union = null;

            foreach (var tre in tokenRegexs) {
                if (!reLex.TryTokenize(tre.Regex, out var reToks, true)) {  // tokenizando regex
                    throw new ArgumentException($"REGEX '{tre.Regex}' could not be tokenized.", nameof(tokenRegexs));
                }
                if (!reParser.TryParse(reToks, out var reAst)) {  // parsean2 regex
                    throw new ArgumentException($"Syntax error in REGEX '{tre.Regex}'.", nameof(tokenRegexs));
                }
                var ctx = new Context();
                if (!reAst.Validate(ctx)) {  // validan2 regex
                    throw new ArgumentException($"Semantic error in REGEX '{tre.Regex}'.", nameof(tokenRegexs));
                }
                var nfa = reVis.Visit(reAst);  // dame el NFA de la regex
                nfa.SetLabel(tre.Token);

                union = union?.Union(nfa) ?? nfa;
            }
            _dfa = new ConverterToDFA(union)
                .ToDFA();
        }

        public void Dispose() {
            NFA.ResetStateCounter();
            DFAState.ResetStateCounter();
        }

        public IEnumerable<Token> Tokenize(string gosCode) {
            gosCode += Environment.NewLine;

            var state = _dfa.Initial;
            var lexeme = new StringBuilder();
            (int SourceIdx, Token.TypeEnum Label)? labMark = null;  // mark d la u'ltima etiketa
            var line = 1u;
            var col = 1u;
            bool? endl = null;  // null cuan2 no he lei'2 ningu'n token en la li'nea actual, true cuan2 c debe poner un endline, false e.o.c.
            bool ignoreRestOfLine = false;

            for (int i = 0; i < gosCode.Length; i++) {
                if (!ignoreRestOfLine) {
                    if (_dfa.Transitions.TryGetValue((state, gosCode[i]), out var next)) {  // hay una transicio'n con el caracter actual
                        lexeme.Append(gosCode[i]);

                        if (!endl.HasValue) {
                            endl = true;
                        }
                        if (_dfa.Labels.TryGetValue(next, out var label)) {  // si el pro'ximo estado tiene etiketa...
                            labMark = (i + 1, label);  // se le suma 1 a i xq estaremos en el esta2 next en la pro'xima iteracio'n
                        }
                        state = next;
                    } else if (labMark.HasValue) {
                        var (lastI, tokenType) = labMark.Value;

                        col -= (uint)lexeme.Length;  // retrocediendo hasta el 1er caracter del lexema

                        if (i != lastI) {
                            lexeme.Remove(lastI, i - lastI);
                        }
                        var tok = new Token(tokenType, line, col, lexeme.ToString());
                        LogToken(tok);
                        yield return tok;

                        i = lastI - 1;  // pa q en la pro'xima iteracio'n sea lastI
                        col += (uint)lexeme.Length - 1;  // posiciona'ndose en el u'ltimo caracter del lexema, pa en la pro'xima iter ser el caracter siguient
                        state = _dfa.Initial;
                        labMark = null;
                        lexeme.Clear();
                    }
                    if (gosCode[i] is '{' or '}' or '\\') {
                        endl = false;  // no c debe poner un token d fin d li'nea cuan2 vengan algunos d esos caracteres
                    }
                }
                if (gosCode[i] == '#') {
                    ignoreRestOfLine = true;
                }
                else if (IsNewLine(gosCode, ref i)) {
                    if (endl.HasValue && endl.Value) {
                        var tok = new Token(Token.TypeEnum.EndOfLine, line, col, Environment.NewLine);
                        LogToken(tok);
                        yield return tok;
                    }
                    endl = null;  // la li'nea c akbo', x tanto, vuelvo a indefinir la bandera
                    line++;
                    col = 0;  // pa q en la pro'xima iteracio'n sea 1
                    ignoreRestOfLine = false;  // cambio d li'nea, bajo la bandera
                }
                col++;
            }
            yield return new Token(Token.TypeEnum.Eof, line, col, "");
        }

        private void LogToken(Token tok) {
            _log?.LogDebug(
                "Token produced. Type: {type}, lexeme: {lex}, line: {l}, column: {c}",
                tok.Type,
                tok.Lexem,
                tok.Line,
                tok.Column);
        }

        private static bool IsNewLine(string str, ref int idx) {
            switch (str[idx]) {
                case '\r' when idx + 1 < str.Length && str[idx + 1] == '\n':  // fin d li'nea en plataformas no-Unix
                    idx++;
                    return true;
                case '\n':
                    return true;
                default:
                    return false;
            }
        }
    }
}
