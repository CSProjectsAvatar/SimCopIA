
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Lexer
{
    // <summary>
    // Clase que representa un Automata Finito No Determinista
    // </summary>
    public class DFA
    {
        static uint _lastState = 0;
        public static uint GetState { get => _lastState++; }
        public Dictionary<(uint, char), DFAState> Transitions { get; set; }

        public DFA()
        {
            Transitions = new Dictionary<(uint, char), DFAState>();
        }
        public static void ResetStateCounter()
        {
            _lastState = 0;
        }
    }
}