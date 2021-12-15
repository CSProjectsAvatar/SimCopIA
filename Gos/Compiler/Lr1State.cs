using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Compiler {
    public class Lr1State : IReadOnlyCollection<Lr1Item> {
        private readonly ISet<Lr1Item> items;

        public Lr1State(IEnumerable<Lr1Item> items) : this(items.ToHashSet()) {

        }

        public Lr1State(ISet<Lr1Item> items) {
            this.items = items;
        }

        public uint Id { get; internal set; }

        public int Count => items.Count;

        public IEnumerator<Lr1Item> GetEnumerator() {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override bool Equals(object obj) {
            return obj is Lr1State other
                && this.items.IsSubsetOf(other.items)
                && other.items.IsSubsetOf(this.items);
        }

        public override int GetHashCode() {
            return this.items.Aggregate(1, (accum, x) => accum *= x.GetHashCode());
        }

        public override string ToString() {
            return $"State {Id}:{Environment.NewLine}{string.Join(Environment.NewLine, items)}";
        }
    }
}