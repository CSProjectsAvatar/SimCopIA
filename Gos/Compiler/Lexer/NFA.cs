
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
            nfa.Transitions.Add((nfa.Initial, null), new List<uint>() { this.Initial, other.Initial});

            //      ... -> q_f1
            //                 \
            //                  q_f3
            //                 /
            //      ... -> q_f2
            nfa.Final = NFA.GetState;
            foreach (var prevFinal in new [] { this.Final, other.Final}) {
                nfa.Transitions.Add((prevFinal, null), new List<uint>() { nfa.Final });
            }

            nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
                                             .Concat(other.Transitions)
                                             .ToDictionary(g => g.Key, g => g.Value);

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
            nfa.Transitions.Add((this.Final, null), new List<uint>() { other.Initial });

            //   ... -> q_f2 
            nfa.Final = other.Final;

            nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
                                             .Concat(other.Transitions)
                                             .ToDictionary(g => g.Key, g => g.Value);
                                             
            return nfa;
        }

        public NFA Maybe()
        {
            var nfa = new NFA();
            var empty = EmptyNFA();

            return this.Union(empty);
        }

        public NFA Mult()
        {
            //  -----------------------
            //  |                     |
            // q_01 -> ... -> q_f1 -> e -> q'_f
            var nfa = new NFA();
            nfa.Final = NFA.GetState;

            Transitions.Add((this.Final, null), new List<uint>() { nfa.Final });

            if(Transitions.TryGetValue((Initial, null), out var nextStates)){
                nextStates.Add(nfa.Final);
            } else {
                Transitions.Add((Initial, null), new List<uint>() { nfa.Final });
            }

            nfa.Transitions = nfa.Transitions.Concat(this.Transitions)
                                             .ToDictionary(g => g.Key, g => g.Value);

            return nfa;
        }

        public NFA Plus()
        {
            return this.Concat(this.Mult());
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
    }
}