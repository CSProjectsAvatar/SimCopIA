using System;
using System.Collections;
using System.Collections.Generic;

namespace Compiler {
    public class Grammar {
        public Grammar(IReadOnlyList<Production> productions, Type initial) {
            if (!initial.IsSubclassOf(typeof(Unterminal))) {
                throw new ArgumentException("Provided type must inherit Unterminal.", nameof(initial));
            }
            Productions = productions;
            Initial = initial;
        }

        public IReadOnlyList<Production> Productions { get; private set; }
        public Type Initial { get; private set; }
    }
}