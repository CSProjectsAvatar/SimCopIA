using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    /// <summary>
    /// Representa el tipo de un token.
    /// </summary>
    public class TokenType : GramSymType {
        public static TokenType Number = new TokenType(Token.TypeEnum.Number);
        public static TokenType Plus = new TokenType(Token.TypeEnum.Plus);
        public static TokenType Epsilon = new TokenType(Token.TypeEnum.Epsilon);

        /// <summary>
        /// *
        /// </summary>
        public static TokenType Times = new TokenType(Token.TypeEnum.Times);
        private readonly Token.TypeEnum token;

        public TokenType(Token.TypeEnum token) {
            this.token = token;
        }

        public override string Name => token.ToString();

        public override string ToString() {
            return this.token.ToString();
        }

        public static implicit operator TokenType(Token.TypeEnum t) {
            return new TokenType(t);
        }

        public static implicit operator Token.TypeEnum(TokenType t) {
            return t.token;
        }
    }
}
