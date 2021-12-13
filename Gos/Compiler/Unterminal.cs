using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public abstract class Unterminal : GramSymbol {
        public Unterminal(params GramSymbol[] derivation) {
            Ast = SetAst(derivation);
        }

        /// <summary>
        /// Devuelve el nodo de AST del no-terminal, producto de reducir su producción 
        /// cuya derivación es <paramref name="derivation"/>.
        /// </summary>
        /// <param name="derivation"></param>
        /// <returns></returns>
        protected abstract AstNode SetAst(GramSymbol[] derivation);

        public AstNode Ast { get; private set; }

        /// <summary>
        /// Crea un <see cref="Unterminal"/> producto de reducir <paramref name="production"/>.
        /// </summary>
        /// <param name="unterminal"></param>
        /// <param name="derivants"></param>
        /// <returns></returns>
        internal static Unterminal FromReduction(Type unterminal, IReadOnlyList<GramSymbol> derivants) {
            // para asegurarnos d q cada subclase tenga el constructor apropiado, creamos un constructor
            // en esta clase y obligamos a implementar el me'todo SetAst
            return Activator.CreateInstance(unterminal, derivants.ToArray())
                as Unterminal;
        }
    }
}