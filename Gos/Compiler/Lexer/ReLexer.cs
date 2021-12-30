using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer {
    /// <summary>
    /// Tokenizador para expresiones regulares.
    /// </summary>
    class ReLexer {
        private readonly IDictionary<char, Token.TypeEnum> _metas;
        private readonly ILogger<ReLexer> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaCharacters">Los caracteres que tienen un significado para el lenguaje en el que se expresan las 
        /// REGEX, relacionados con el tipo de token correspondiente. Por ejemplo, a este diccionario pueden pertenecer el 
        /// caracter *, que corresponde al operador 
        /// clausura, relacionado con el tipo <see cref="Token.TypeEnum.Times"/>.</param>
        public ReLexer(IDictionary<char, Token.TypeEnum> metaCharacters, ILogger<ReLexer> logger) {
            _metas = metaCharacters;
            _log = logger;
        }

        /// <summary>
        /// Inicializa un nuevo lexer de REGEX con los meta-caracteres siguientes:
        /// <code>* + - [ ] ( ) | ? \</code>
        /// cuyos tipos de token son los triviales.
        /// </summary>
        /// <param name="logger"></param>
        public ReLexer(ILogger<ReLexer> logger) : this(new Dictionary<char, Token.TypeEnum> {
                    ['*'] = Token.TypeEnum.Times,
                    ['+'] = Token.TypeEnum.Plus,
                    ['-'] = Token.TypeEnum.Minus,
                    ['['] = Token.TypeEnum.LBracket,
                    [']'] = Token.TypeEnum.RBracket,
                    ['('] = Token.TypeEnum.LPar,
                    [')'] = Token.TypeEnum.RPar,
                    ['|'] = Token.TypeEnum.Pipe,
                    ['?'] = Token.TypeEnum.Quest,
                    ['\\'] = Token.TypeEnum.Scape,
                },
                logger) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="tokens"></param>
        /// <param name="appendEof">Si se quiere que el último token sea un token de fin de archivo.</param>
        /// <returns></returns>
        public bool TryTokenize(string regex, out IEnumerable<Token> tokens, bool appendEof = false) {
            var list = new List<Token>();
            tokens = list;

            for (int i = 0; i < regex.Length; i++) {
                var c = regex[i];

                if (c == '\\') {  // c = '\', el caracter q escapea
                    if (i + 1 >= regex.Length) {
                        _log.LogError(
                            "Meta-character to scape expected at element number {col} of regex, but string is over.", 
                            i + 2);
                        return false;
                    }
                    if (!_metas.ContainsKey(regex[i + 1])) {  // el caracter a escapear no es meta
                        _log.LogWarning("The scaped character {char} at column {col} is not a meta-character.",
                            regex[i+1],
                            i + 2);
                    }
                    list.Add(new Token(Token.TypeEnum.Char, 1, (uint)(i + 1), regex[i + 1].ToString()));
                    i++;  // omitir el caracter siguiente, xq ya esta' escapeao
                } else if (_metas.TryGetValue(c, out var tokenType)) {  // c es un meta-caracter
                    list.Add(new Token(tokenType, 1, (uint)(i + 1), c.ToString()));
                } else {  // c es un caracter q no tiene significado especial para el lenguaje de las REGEX 
                    list.Add(new Token(Token.TypeEnum.Char, 1, (uint)(i + 1), c.ToString()));
                }
            }
            if (appendEof) {
                list.Add(new Token(Token.TypeEnum.Eof, 1, (uint)regex.Length, ""));
            }
            return true;
        }
    }
}
