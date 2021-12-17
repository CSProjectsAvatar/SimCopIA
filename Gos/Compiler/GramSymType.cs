using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    /// <summary>
    /// Representa el tipo de un símbolo de la gramática.
    /// </summary>
    public abstract class GramSymType {
        public abstract string Name { get; }

        public static (GramSymType, TokenType, GramSymType) operator +(GramSymType @this, GramSymType other) {
            return (@this, TokenType.Plus, other);
        }

        public static (GramSymType, TokenType, GramSymType) operator *(GramSymType @this, GramSymType other) {
            return (@this, TokenType.Times, other);
        }

        public override bool Equals(object obj) {
            return obj is GramSymType other && this.Name == other.Name;
        }

        public override int GetHashCode() {
            return this.Name.GetHashCode();
        }
    }
}
