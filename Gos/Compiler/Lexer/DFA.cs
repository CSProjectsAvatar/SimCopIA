
using System;
using System.Collections.Generic;
using System.Linq;

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
        public uint Initial { get; set; }
        public List<uint> FinalStates { get; set; }
        public List<DFAState> States { get; set; }
        public DFA()
        {
            Transitions = new Dictionary<(uint, char), uint>();
            FinalStates = new List<uint>();
        }
        public static void ResetStateCounter()
        {
            _lastState = 0;
        }
    }
}