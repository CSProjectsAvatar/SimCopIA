
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Lexer
{
    // <summary>
    // Clase que representa un Automata Finito No Determinista
    // </summary>
    public class ConverterToDFA
    {
        private NFA automat;
        public ConverterToDFA(NFA atomaton){
            this.automat = atomaton;
        }
        public DFAState eClosure(uint state){
            return eClosure(new[] { state });
        }
        public DFAState eClosure(IEnumerable<uint> items) {
            var q = new Queue<uint>(items);
            var set = new HashSet<uint>(items);

            while (q.Count != 0) {
                var item = q.Dequeue();

                automat.Transitions.TryGetValue((item, null), out var next);
                if (next is null) continue;

                foreach (var nextItem in next) {
                    if (!set.Contains(nextItem)) {
                        set.Add(nextItem);
                        q.Enqueue(nextItem);
                    }
                }
            }
            return new DFAState(set);
        }
        public DFAState GoTo(DFAState state, char c){
            var set = new HashSet<uint>();
            foreach (var item in state.MicroStates) {
                automat.Transitions.TryGetValue((item, c), out var next); // f(q_i, c)
                if (next is null) continue;

                foreach (var nextItem in next) { // q_j in f(q_i, c)
                    set.Add(nextItem);
                }
            }
            return new DFAState(set);
        }
    
        public DFA ToDFA(){
            var dfa = new DFA();
            var q0 = eClosure(automat.Initial);

            var q = new Queue<DFAState>();
            q.Enqueue(q0);

            var set = new HashSet<DFAState>();
            set.Add(q0);

            while (q.Count != 0) {
                var q_i = q.Dequeue();
                foreach (char c in automat.Transitions.Keys.Select(g => g.Item2).Distinct()) {
                    var q_next = eClosure(GoTo(q_i, c).MicroStates); // Qj = eClosure(Goto(Qj, c)).
                    if (set.Contains(q_next.StNumber)) continue; // @audit Hacer bien con subc( C )

                    q.Enqueue(q_next);
                    set.Add(q_next);
                    dfa.Transitions.Add((q_i.StNumber, c), q_next); // @audit ver lo de que los estados finales sean los que tienen alguien q pert a un estado final del original
                }
            }
            return dfa;
        }
    }

    
}