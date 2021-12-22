using Compiler;
using Compiler.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class NFAConverterTests : CompilerTests {

        // Test que prueba la clausura del primer estado de un NFA
        [TestMethod]
        public void ClousureFirstNFAState() {
            NFA.ResetStateCounter();

            var nfa = new NFA('a');

            var dfa = new ConverterToDFA(nfa).Closure(nfa.Initial);

            Assert.AreEqual(1, dfa.Count);
            Assert.IsTrue(dfa.Contains(nfa.Initial));
        }

         // Test que prueba la clausura del primer estado de un NFA Union
        [TestMethod]
        public void ClousureUnionFirstNFAState() {
            NFA.ResetStateCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            var firstClousure = new ConverterToDFA(nfaUnion).Closure(nfaUnion.Initial);

            Assert.AreEqual(3, firstClousure.Count);
            Assert.IsTrue(firstClousure.Contains(nfa1.Initial));
            Assert.IsTrue(firstClousure.Contains(nfa2.Initial));
            
        }

        // Test que prueba la clausura del primer estado de un NFA Multiplicacion
        [TestMethod]
        public void ClousureMultiplicationFirstNFAState() {
            NFA.ResetStateCounter();

            var nfa1 = new NFA('a');

            var firstClnfaMult = nfa1.Mult();

            var firstClousure = new ConverterToDFA(firstClnfaMult).Closure(firstClnfaMult.Initial);

            Assert.AreEqual(2, firstClousure.Count);
            Assert.IsTrue(firstClousure.Contains(nfa1.Initial));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 1));
        }
        
        // Test que prueba la clausura del primer estado de un NFA Maybe
        [TestMethod]
        public void ClousureMaybeFirstNFAState() {
            NFA.ResetStateCounter();

            var nfa1 = new NFA('a');

            var firstClnfaMaybe = nfa1.Maybe();

            var firstClousure = new ConverterToDFA(firstClnfaMaybe).Closure(firstClnfaMaybe.Initial);

            Assert.AreEqual(4, firstClousure.Count);
            Assert.IsTrue(firstClousure.Contains(nfa1.Initial));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 1));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 2));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 3));
        }
        
        // Test que prueba el Goto del primer estado de un NFA Union
        [TestMethod]
        public void GotoUnionFirstNFAState() {
            NFA.ResetStateCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            var firstClousure = new ConverterToDFA(nfaUnion).Closure(nfaUnion.Initial);

            var firstGoto = new ConverterToDFA(nfaUnion).GoTo(firstClousure, 'a');

            Assert.AreEqual(1, firstGoto.Count);
            Assert.IsTrue(firstGoto.Contains(nfa1.Final));

            var secondGoto = new ConverterToDFA(nfaUnion).GoTo(firstClousure, 'b');

            Assert.AreEqual(1, secondGoto.Count);
            Assert.IsTrue(secondGoto.Contains(nfa2.Final));
        }
    
    }
}
