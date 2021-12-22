
using System;
using System.Collections.Generic;

namespace Compiler.Lexer
{
    public class Automaton {
        internal static Automaton FromChar(char value) {
            throw new NotImplementedException();
        }

        internal Automaton Union(Automaton automaton) {
            throw new NotImplementedException();
        }

        internal Automaton Maybe() {
            throw new NotImplementedException();
        }

        internal Automaton Plus() {
            throw new NotImplementedException();
        }

        internal Automaton Concat(Automaton automaton) {
            throw new NotImplementedException();
        }

        internal Automaton Mult() {
            throw new NotImplementedException();
        }
    }
}