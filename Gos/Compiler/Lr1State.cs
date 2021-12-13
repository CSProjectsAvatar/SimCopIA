using System.Collections;
using System.Collections.Generic;

namespace Compiler {
    internal class Lr1State : IEnumerable<Lr1Item> {
        private readonly IEnumerable<Lr1Item> items;

        public Lr1State(IEnumerable<Lr1Item> items) {
            this.items = items;
        }

        public uint Id { get; internal set; }

        public IEnumerator<Lr1Item> GetEnumerator() {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}