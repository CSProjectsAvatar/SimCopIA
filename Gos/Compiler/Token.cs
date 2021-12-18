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

        public enum TypeEnum {
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
            LessThan,

            /// <summary>
            /// >
            /// </summary>
            GreaterThan,

            Minus,

            /// <summary>
            /// /
            /// </summary>
            Div,

            /// <summary>
            /// Identificador.
            /// </summary>
            Id,

            If,
            Return
        }

        public override string ToString() {
            return Type.ToString();
        }
    }
}