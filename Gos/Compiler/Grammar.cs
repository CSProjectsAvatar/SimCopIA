using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public class Grammar {
        public Grammar(IEnumerable<Production> productions, UntType initial) {
            this.productions = productions.ToList();
            Initial = initial;
        }

        public Grammar(UntType initial, params Production[] productions) : this(productions, initial) {

        }

        public IReadOnlyList<Production> Productions => productions;

        /// <summary>
        /// Símbolo inicial de la gramática (S).
        /// </summary>
        public UntType Initial { get; internal set; }


        /// <summary>
        /// Añade una producción a la lista de producciones. Este método sólo puede ser
        /// invocado una vez, ya que su propósito es añadir una producción ficticia de la forma
        /// <code>S' -> E</code>
        /// donde E es <see cref="Initial"/> y S' es un nuevo símbolo.
        /// </summary>
        internal Action<Production> AddFakeProduction => 
            Helper.Once((Production p) => this.productions.Add(p));
        
        private List<Production> productions;
    }
}