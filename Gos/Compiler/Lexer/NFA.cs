
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Lexer
{
    // <summary>
    // Clase que representa un Automata Finito No Determinista
    // </summary>
    public class NFA
    {
        static uint _lastState = 0;
        public static uint GetState { get => _lastState++; }
        // public ICollection<uint> States { get; set; } //@audit Pa que?
        public Dictionary<(uint, char?), List<uint>> Transitions { get; set; }
        public uint Initial { get; set; }
        public uint Final { get; set; }

        public NFA()
        {
            Transitions = new Dictionary<(uint, char?), List<uint>>();
        }
        public NFA(char c) : this()
        {
            Initial = NFA.GetState;
            Final = NFA.GetState;

            Transitions.Add((Initial, c), new List<uint>() { Final });
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
            // nfa.Transitions.Add((nfa.Initial, null), new List<uint>() { this.Initial, other.Initial});

            //      ... -> q_f1
            //                 \
            //                  q_f3
            //                 /
            //      ... -> q_f2
            nfa.Final = NFA.GetState;
            nfa.AddTransition(this.Final, null, new uint[] {nfa.Final});
            nfa.AddTransition(other.Final, null, new uint[] {nfa.Final});

            // foreach (var prevFinal in new [] { this.Final, other.Final}) {
            //     nfa.Transitions.Add((prevFinal, null), new List<uint>() { nfa.Final });
            // }

            foreach (var tr in this.Transitions.Concat(other.Transitions))
            {
                nfa.AddTransition(tr.Key.Item1, tr.Key.Item2, tr.Value);
            }
            // nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
            //                                  .Concat(other.Transitions)
            //                                  .ToDictionary(g => g.Key, g => g.Value);

            return nfa;
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

        // Le suma un valor a todos los valores de las transiciones, necesario para el Plus
        private void UpFloor()
        {
            var floor = _lastState;
            this.Initial += floor;
            this.Final += floor;

            var newDict = new Dictionary<(uint, char?), List<uint>>();

            foreach (var tr in Transitions)
            {
                var newKey = (tr.Key.Item1 + floor, tr.Key.Item2);
                var newValue = tr.Value.Select(v => v + floor).ToList();
                newDict.Add(newKey, newValue);
            }

            Transitions = newDict;
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