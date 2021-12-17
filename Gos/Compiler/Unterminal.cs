using System;
using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    public abstract class Unterminal : GramSymbol {

        /// <summary>
        /// Devuelve el nodo de AST del no-terminal, producto de reducir su producción 
        /// cuya derivación es <paramref name="derivation"/>.
        /// </summary>
        /// <param name="derivation"></param>
        /// <returns></returns>
        protected abstract AstNode SetAst(IReadOnlyList<GramSymbol> derivation);

        public AstNode Ast { get; private set; }

        /// <summary>
        /// Crea un <see cref="Unterminal"/> producto de reducir <paramref name="production"/>.
        /// </summary>
        /// <param name="unterminal"></param>
        /// <param name="derivants"></param>
        /// <returns></returns>
        internal static Unterminal FromReduction(UntType unterminal, IReadOnlyList<GramSymbol> derivants) {
            var res = Activator.CreateInstance((Type)unterminal) as Unterminal;
            res.Ast = res.SetAst(derivants);

            return res;
        }
    }
}