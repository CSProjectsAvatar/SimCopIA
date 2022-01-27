
using System;
using System.Collections.Generic;
using System.Linq;
using static Compiler.Token;

namespace Compiler.Lexer
{
    // <summary>
    // Convertidor de aut�mata no determinista a uno determinista.
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
            dfa.States.Add(q0);

            var q = new Queue<DFAState>();
            q.Enqueue(q0);

            var set = new HashSet<string>();
            set.Add(q0.StringHash());

            while (q.Count != 0) {
                var q_i = q.Dequeue();
                var chars = automat.Transitions.Keys.Where(g => g.Item2 is not null).Select(g => g.Item2).Distinct();
                
                foreach (char c in chars) {
                    var q_next = eClosure(GoTo(q_i, c).MicroStates); // Qj = eClosure(Goto(Qj, c)).

                    if(!q_next.Empty){
                        if (!set.Contains(q_next.StringHash())) { // Qj no esta en Q, no lo he analizado
                            q.Enqueue(q_next);
                            set.Add(q_next.StringHash());
                            dfa.States.Add(q_next);
                            dfa.Transitions.Add((q_i.StNumber, c), q_next.StNumber); 
                        }
                        else { // Qj esta en Q, lo he analizado
                            var q_old = dfa.States.Where(q => q.StringHash() == q_next.StringHash()).First();
                            dfa.Transitions.Add((q_i.StNumber, c), q_old.StNumber);
                        }
                    }
                }
            }

            MarkFinalStates(dfa);
            MarkLabels(dfa);
            return dfa;
            
        }

        // Asigna a cada estado del DFA una etiqueta del dict Labels, la menor de las que le corresponden a cada microestado
        private void MarkLabels(DFA dfa)
        {
            foreach (var state in dfa.States) // Qi
            {
                var labels = new List<TypeEnum>();
                foreach (var microState in state.MicroStates) // q_i
                {
                    automat.Labels.TryGetValue(microState, out var labelList);
                    if (labelList is not null){
                        labels.AddRange(labelList); // labels = labels U labels(q_i)
                    }
                   
                }
                if (labels.Count != 0){
                    dfa.Labels[state.StNumber] = labels.Min(); // labels(Qi) = min(labels)
                }
                   
            }

        }

        // Marca como finales los estados del dfa que contienen algun microestado final del automat
        internal void MarkFinalStates(DFA dfa){
            foreach (var state in dfa.States) {
                if (state.Contains(automat.Final)) {
                    dfa.FinalStates.Add(state.StNumber);
                }
            }
        }
    }

    
}