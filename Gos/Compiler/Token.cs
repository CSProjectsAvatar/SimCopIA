using System;

namespace Compiler {
    public class Token : GramSymbol {
        private static uint colHelper;  // mantiene una columna ficticia q va aumentando (ver Eof)

        public string Lexem { get; internal set; }
        public TypeEnum Type { get; internal set; }
        public uint Line { get; internal set; }
        public uint Column { get; internal set; }

        public Token(TypeEnum type, uint line, uint column, string lex) {
            Type = type;
            Line = line;
            Column = column;
            Lexem = lex;
        }

        internal static Token CharFor(char v) {
            return new Token(TypeEnum.Char, 1, ++colHelper, v.ToString());
        }

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Eof => new Token(TypeEnum.Eof, 1, ++colHelper, "$");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Epsilon => new Token(TypeEnum.Epsilon, 1, ++colHelper, "");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Number => new Token(TypeEnum.Number, 1, ++colHelper, "5");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Plus => new Token(TypeEnum.Plus, 1, ++colHelper, "+");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Eq => new Token(TypeEnum.Eq, 1, ++colHelper, "=");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Let => new Token(TypeEnum.Let, 1, ++colHelper, "let");

        /// <summary>
        /// Crea un identificador con el nombre dado (<paramref name="id"/>).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static Token IdFor(string id) {
            return new Token(TypeEnum.Id, 1, ++colHelper, id);
        }

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Print => new Token(TypeEnum.Print, 1, ++colHelper, "print");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token VarX => new Token(TypeEnum.Id, 1, ++colHelper, "x");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Endl => new Token(TypeEnum.EndOfLine, 1, ++colHelper, Environment.NewLine);

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Times => new Token(TypeEnum.Times, 1, ++colHelper, "*");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token LPar => new Token(TypeEnum.LPar, 1, ++colHelper, "(");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token RPar => new Token(TypeEnum.RPar, 1, ++colHelper, ")");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Minus => new Token(TypeEnum.Minus, 1, ++colHelper, "-");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Fun => new Token(TypeEnum.Fun, 1, ++colHelper, "fun");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token LBrace => new Token(TypeEnum.LBrace, 1, ++colHelper, "{");

        internal static string GetDefaultLexeme(TypeEnum token) {
            // @note SOLO HACEN FALTA ANYADIR AQUELLOS CUYO NOMBRE D TIPO NO ES LO SUFICIENTEMENT EXPLI'CITO, X EJEMPLO, NO HAC
            // FALTA ANYADIR TypeEnum.Number
            return token switch {  // @remind ANYADIR NUEVOS ELEMENTOS AL SWITCH PARA SOPORTAR UN MEJOR FEEDBACK CUAN2 OCURRAN ERRORES D SINTAXIS
                TypeEnum.Comma => ",",
                TypeEnum.Div => "/",
                TypeEnum.EndOfLine => throw new ArgumentException(
                    "Lexeme for end-of-line token can't be provided.", 
                    nameof(token)),
                TypeEnum.Eq => "=",
                TypeEnum.EqEq => "==",
                TypeEnum.GreaterOrEqualThan => ">=",
                TypeEnum.GreaterThan => ">",
                TypeEnum.Id => "Identifier",
                TypeEnum.LBrace => "{",
                TypeEnum.LBracket => "[",
                TypeEnum.LowerThan => "<",
                TypeEnum.LPar => "(",
                TypeEnum.Minus => "-",
                TypeEnum.NotEq => "!=",
                TypeEnum.Pipe => "|",
                TypeEnum.Plus => "+",
                TypeEnum.Quest => "?",
                TypeEnum.RBrace => "}",
                TypeEnum.RBracket => "]",
                TypeEnum.RPar => ")",
                TypeEnum.Scape => "\\",
                TypeEnum.Times => "*",
                _ => token.ToString()
            };
        }

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token RBrace => new Token(TypeEnum.RBrace, 1, ++colHelper, "}");

        /// <summary>
        /// Propiedad helper para debuguear y testear (<).
        /// </summary>
        internal static Token Lt => new Token(TypeEnum.LowerThan, 1, ++colHelper, "<");

        /// <summary>
        /// Propiedad helper para debuguear y testear (>=).
        /// </summary>
        internal static Token Geq => new Token(TypeEnum.GreaterOrEqualThan, 1, ++colHelper, ">=");

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token If => new Token(TypeEnum.If, 1, ++colHelper, "if");

        /// <summary>
        /// Propiedad helper para debuguear y testear (>).
        /// </summary>
        internal static Token Gt => new Token(TypeEnum.GreaterThan, 1, ++colHelper, ">");

        /// <summary>
        /// Propiedad helper para debuguear y testear (==).
        /// </summary>
        internal static Token EqEq => new Token(TypeEnum.EqEq, 1, ++colHelper, "==");

        /// <summary>
        /// Propiedad helper para debuguear y testear (==).
        /// </summary>
        internal static Token Return => new Token(TypeEnum.Return, 1, ++colHelper, "return");

        /// <summary>
        /// Propiedad helper para debuguear y testear (|).
        /// </summary>
        internal static Token Pipe => new Token(TypeEnum.Pipe, 1, ++colHelper, "|");

        /// <summary>
        /// Propiedad helper para debuguear y testear ([).
        /// </summary>
        internal static Token LBracket => new Token(TypeEnum.LBracket, 1, ++colHelper, "[");

        /// <summary>
        /// Propiedad helper para debuguear y testear (]).
        /// </summary>
        internal static Token RBracket => new Token(TypeEnum.RBracket, 1, ++colHelper, "]");

        /// <summary>
        /// Propiedad helper para debuguear y testear (?).
        /// </summary>
        internal static Token Quest => new Token(TypeEnum.Quest, 1, ++colHelper, "?");

        /// <summary>
        /// Propiedad helper para debuguear y testear (/).
        /// </summary>
        internal static Token Div => new Token(TypeEnum.Div, 1, ++colHelper, "/");

        internal static Token NumberFor(double x) {
            return new Token(TypeEnum.Number, 1, ++colHelper, x.ToString());
        }

        public enum TypeEnum {  // @remind LOS TOKENS TIENEN Q APARECER DEL + PRIORITARIO AL -
            /// <summary>
            /// Fin de archivo.
            /// </summary>
            Eof,

            /// <summary>
            /// Para uso del parser.
            /// </summary>
            Epsilon,
            Number,
            Plus,

            /// <summary>
            /// *
            /// </summary>
            Times,

            /// <summary>
            /// (
            /// </summary>
            LPar,

            /// <summary>
            /// )
            /// </summary>
            RPar,

            /// <summary>
            /// =
            /// </summary>
            Eq,

            /// <summary>
            /// ;
            /// </summary>
            EndOfLine,

            /// <summary>
            /// Definición de variable.
            /// </summary>
            Let,

            /// <summary>
            /// Definición de función.
            /// </summary>
            Fun,

            /// <summary>
            /// {
            /// </summary>
            LBrace,

            /// <summary>
            /// }
            /// </summary>
            RBrace,

            Print,
            Comma,

            /// <summary>
            /// <
            /// </summary>
            LowerThan,

            /// <summary>
            /// >
            /// </summary>
            GreaterThan,

            Minus,

            /// <summary>
            /// /
            /// </summary>
            Div,

            If,
            Return,

            /// <summary>
            /// Identificador.
            /// </summary>
            Id,
            GreaterOrEqualThan,

            /// <summary>
            /// ==
            /// </summary>
            EqEq,

            /// <summary>
            /// !=
            /// </summary>
            NotEq,

            /// <summary>
            /// ?
            /// </summary>
            Quest,

            /// <summary>
            /// [
            /// </summary>
            LBracket,

            /// <summary>
            /// ]
            /// </summary>
            RBracket,

            /// <summary>
            /// Representa a un caracter cualquiera. Su lexema debe contener sólo al caracter que representa.
            /// </summary>
            Char,

            /// <summary>
            /// |
            /// </summary>
            Pipe,

            /// <summary>
            /// \
            /// </summary>
            Scape
        }

        public override string ToString() {
            return Type.ToString();
        }
    }
}