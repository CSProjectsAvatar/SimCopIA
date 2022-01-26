
using System;
using System.Collections.Generic;
using System.Linq;
using static Compiler.Token;

namespace Compiler.Lexer
{
    // <summary>
    // Clase que representa un Automata Finito Determinista
    // </summary>
    public class DFA
    {
        static uint _lastState = 0;
        public static uint GetState { get => _lastState++; }
        public Dictionary<(uint, char), uint> Transitions { get; set; }
        public Dictionary<uint, TypeEnum> Labels { get; set; }
        public uint Initial { get; set; }
        public List<uint> FinalStates { get; set; }
        public List<DFAState> States { get; set; }
        public DFA()
        {
            Transitions = new Dictionary<(uint, char), uint>();
            Labels = new Dictionary<uint, TypeEnum>();
            FinalStates = new List<uint>();
            States = new List<DFAState>();
        }
        public static void ResetStateCounter()
        {
            _lastState = 0;
        }
    
        public bool Accept(string s){
            var state = Initial;
            foreach (var c in s)
            {
                if (!Transitions.TryGetValue((state, c), out var next))
                    return false;
                state = next;
            }
            return FinalStates.Contains(state);
        }

        public bool Accept(string s, TypeEnum label){
            var state = Initial;
            foreach (var c in s)
            {
                if (!Transitions.TryGetValue((state, c), out var next))
                    return false;
                state = next;
            }
            return FinalStates.Contains(state) && Labels[state] == label;

        }
        
        public override string ToString()
        {
            var s = string.Empty;
            foreach (var tr in Transitions)
            {
                s += $"{tr.Key.Item1}, {tr.Key.Item2} -> {tr.Value} | " ;
            }
            return s;
        }
    }
}