using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public class Production {

        public Production(uint id, Type unterminal, params Type[] derivation) {
            if (derivation.Any(t => !t.IsSubclassOf(typeof(GramSymbol)))) {
                throw new ArgumentException($"Provided types must derive {nameof(GramSymbol)}.", 
                    nameof(unterminal));
            }
            if (!unterminal.IsSubclassOf(typeof(Unterminal))) {
                throw new ArgumentException($"Provided type must derive {nameof(Compiler.Unterminal)}.", 
                    nameof(unterminal));
            } 
            Derivation = derivation;
            Unterminal = unterminal;
            Id = id;
        }

        public IReadOnlyList<Type> Derivation { get; private set; }
        public Type Unterminal { get; private set; }
        public uint Id { get; private set; }
    }
}