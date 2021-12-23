
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Lexer
{
    public class DFAState
    {
        static uint _lastState = 0;
        private string _getHashCode = "";

        public uint StNumber { get; }
        private List<uint> mStates { get; set; }
        internal IEnumerable<uint> MicroStates { get => mStates; }
        public int Count => mStates.Count;
        public DFAState(IEnumerable<uint> microStates) {
            this.mStates = microStates.ToList();
            this.mStates.Sort();
            this.StNumber = _lastState++;
        }
        public bool Contains(uint microState) {
            return MicroStates.Contains(microState);
        }

        
        public string StringHash()
        {
            if(_getHashCode == string.Empty){
                foreach (var miniSt in MicroStates) {
                    _getHashCode += miniSt;
                }
            }
            return _getHashCode;
        }
    }
}