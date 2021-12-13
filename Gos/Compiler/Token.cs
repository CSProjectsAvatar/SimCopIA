using System;

namespace Compiler {
    public class Token : GramSymbol {
        private static uint colHelper;  // mantiene una columna ficticia q va aumentando (ver Eof)

        public TypeEnum Type { get; internal set; }
        public uint Line { get; internal set; }
        public uint Column { get; internal set; }

        public Token(TypeEnum type, uint line, uint column) {
            Type = type;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Eof => new Token(TypeEnum.Eof, 1, ++colHelper);

        /// <summary>
        /// Propiedad helper para debuguear y testear.
        /// </summary>
        internal static Token Epsilon => new Token(TypeEnum.Epsilon, 1, ++colHelper);

        public enum TypeEnum {
            Eof,  // fin de archivo
            Epsilon  // para uso del parser
        }
    }
}