using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public class Production {
        private static uint id = 0;

        public Production(UntType unterminal, params GramSymType[] derivation) {
            Derivation = derivation;
            Unterminal = unterminal;
            Id = ++id;
        }

        public IReadOnlyList<GramSymType> Derivation { get; private set; }
        public UntType Unterminal { get; private set; }
        public uint Id { get; private set; }

        public override string ToString() {
            return $"{Unterminal} -> " + string.Join(' ', Derivation);
        }

        public override bool Equals(object obj) {
            return obj is Production other
                && this.Unterminal == other.Unterminal
                && this.Derivation.Count == other.Derivation.Count
                && this.Derivation.Zip(other.Derivation)
                    .All(pair => pair.First.Equals(pair.Second));
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.Unterminal, this.Derivation);
        }
    }
}