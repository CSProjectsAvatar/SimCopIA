
using System;
using System.Collections.Generic;
using System.Linq;
using static Compiler.Token;

namespace Compiler.Lexer
{
    // <summary>
    // Clase que representa un Automata Finito No Determinista
    // </summary>
    public class NFA
    {
        static uint _lastState = 0;
        public static uint GetState { get => _lastState++; }
        public Dictionary<(uint, char?), List<uint>> Transitions { get; set; }
        public Dictionary<uint, List<TypeEnum>> Labels { get; set; }
        public uint Initial { get; set; }
        public uint Final { get; set; }

        public NFA()
        {
            Transitions = new Dictionary<(uint, char?), List<uint>>();
            Labels = new Dictionary<uint, List<TypeEnum>>();
        }
        public NFA(char c) : this()
        {
            Initial = NFA.GetState;
            Final = NFA.GetState;

            Transitions.Add((Initial, c), new List<uint>() { Final });
        }

        /// <summary>
        /// Asigna una etiqueta al estado final del automata
        /// </summary>
        public void SetLabel(TypeEnum label){
            AddLabel(Final, new [] { label });
        }
        
        public NFA Union(NFA other)
        {
            var nfa = new NFA();
            nfa.Initial = NFA.GetState;

            //      q_01 -> ...
            //     /
            // q_03 
            //     \
            //      q_02 -> ...
            nfa.AddTransition(nfa.Initial, null, new uint[] {this.Initial, other.Initial});

            //      ... -> q_f1
            //                 \
            //                  q_f3
            //                 /
            //      ... -> q_f2
            nfa.Final = NFA.GetState;
            nfa.AddTransition(this.Final, null, new uint[] {nfa.Final});
            nfa.AddTransition(other.Final, null, new uint[] {nfa.Final});

            foreach (var tr in this.Transitions.Concat(other.Transitions))
            {
                nfa.AddTransition(tr.Key.Item1, tr.Key.Item2, tr.Value);
            }

            nfa.JoinLabels(this, other);
            return nfa;
        }

        // Aniade a this los Labels de nfaA y nfaB
        private void JoinLabels(NFA nfaA, NFA nfaB)
        {
            foreach (var label in nfaA.Labels.Concat(nfaB.Labels))
            {
                AddLabel(label.Key, label.Value);
            }
        }

        internal static NFA FromChar(char c) {
            return new NFA(c);
        }

        public NFA Concat(NFA other)
        {
            var nfa = new NFA();

            //   q_01 -> ...
            nfa.Initial = this.Initial;

            //   ... -> q_f1 -> e -> q_02 -> ... 
            nfa.AddTransition(this.Final, null, new uint[] { other.Initial});
            // nfa.Transitions.Add((this.Final, null), new List<uint>() { other.Initial });

            //   ... -> q_f2 
            nfa.Final = other.Final;

            foreach (var tr in this.Transitions.Concat(other.Transitions))
            {
                nfa.AddTransition(tr.Key.Item1, tr.Key.Item2, tr.Value);
            }

            // nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
            //                                  .Concat(other.Transitions)
            //                                  .ToDictionary(g => g.Key, g => g.Value);
                                             
            return nfa;
        }

        public NFA Maybe()
        {
            var empty = EmptyNFA();

            return this.Union(empty);
        }

        public NFA Mult()
        {
            //(Final)
            // q_01 -> ... -> q_f1
            //  |              |       
            //  <---------------

            var nfa = new NFA();
            nfa.Initial = this.Initial;
            nfa.Final = nfa.Initial;

            nfa.AddTransition(this.Final, null, new uint[] {nfa.Final});
            // nfa.Transitions.Add((this.Final, null), new List<uint>() { nfa.Initial });

            foreach (var tr in this.Transitions)
            {
                nfa.AddTransition(tr.Key.Item1, tr.Key.Item2, tr.Value);
            }

            // nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
            //                                  .ToDictionary(g => g.Key, g => g.Value);

            return nfa;
        }

        public NFA Plus()
        {
            //              (Final)
            // q_01 -> ... -> q_f1
            //  |              |       
            //  <----- e -------
            var nfa = new NFA();
            nfa.Initial = this.Initial;
            nfa.Final = this.Final;


            // nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
            //                                  .ToDictionary(g => g.Key, g => g.Value);
            
            nfa.AddTransition(this.Final, null, new uint[] {nfa.Initial});
            // nfa.Transitions.Add((this.Final, null), new List<uint>() { nfa.Initial });

            foreach (var tr in this.Transitions)
            {
                nfa.AddTransition(tr.Key.Item1, tr.Key.Item2, tr.Value);
            }
            return nfa;
        }

        private void AddTransition(uint from, char? c, IEnumerable<uint> to)
        {
            var key = (from, c);
            if (!Transitions.ContainsKey(key))
            {
                Transitions.Add(key, new List<uint>());
            }
            Transitions[key].AddRange(to);
        }
        private void AddLabel(uint state, IEnumerable<TypeEnum> labels)
        {
            if (!Labels.ContainsKey(state))
            {
                Labels.Add(state, new List<TypeEnum>());
            }
            Labels[state].AddRange(labels);
        }
        

        public static NFA EmptyNFA()
        {
            var nfa = new NFA();
            nfa.Initial = nfa.Final = NFA.GetState;
            return nfa;
        }
        public static void ResetStateCounter()
        {
            _lastState = 0;
        }

        public override string ToString()
        {
            var s = string.Empty;
            foreach (var tr in Transitions)
            {
                s += $"{tr.Key.Item1}, {tr.Key.Item2} -> {string.Join(", ", tr.Value.ForPrint())} | " ;
            }
            return s;
        }
    }

 
    public static class AutomatExtensions
    {
        public static string ForPrint(this IEnumerable<uint> states)
        {
            return string.Join(", ", states.Select(s => s.ToString()));
        }
    }
}