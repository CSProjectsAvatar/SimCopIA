
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Lexer
{
    public class DFAState
    {
        static uint _lastState = 0;
        public uint StNumber { get; }
        internal List<uint> MicroStates { get; set; }
        public int Count => MicroStates.Count;
        public DFAState(IEnumerable<uint> microStates) {
            this.MicroStates = microStates.ToList();
            this.StNumber = _lastState++;
        }
        public bool Contains(uint microState) {
            return MicroStates.Contains(microState);
        }
    }
}