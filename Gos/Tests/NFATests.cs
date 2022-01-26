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
    public class NFATest : CompilerTests {

        // Test que prueba la union de 2 NFA
        [TestMethod]
        public void Union2NFA() {
            NFA.ResetStateCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            Assert.AreEqual((uint)4, nfaUnion.Initial);

            Assert.AreEqual((uint)5, nfaUnion.Final);

            Assert.IsFalse(nfaUnion.Transitions.ContainsKey((nfaUnion.Initial, 'a')));
            Assert.IsFalse(nfaUnion.Transitions.ContainsKey((nfaUnion.Initial, 'b')));
            
            Assert.IsTrue(nfaUnion.Transitions.ContainsKey((nfa1.Initial, 'a')));
            Assert.IsTrue(nfaUnion.Transitions.ContainsKey((nfa2.Initial, 'b')));
        }

        // Test que prueba la concatenacion de 2 NFA
        [TestMethod]
        public void Concat2NFA() {
            NFA.ResetStateCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaConcat = nfa1.Concat(nfa2);

            Assert.AreEqual((uint)0, nfaConcat.Initial);

            Assert.AreEqual((uint)3, nfaConcat.Final);

            Assert.IsTrue(nfaConcat.Transitions.ContainsKey((nfaConcat.Initial, 'a')));
            Assert.IsFalse(nfaConcat.Transitions.ContainsKey((nfaConcat.Initial, 'b')));
            
            Assert.IsTrue(nfaConcat.Transitions.ContainsKey((1, null)));
        }

        [TestMethod]
        public void MaybeNFA(){
            NFA.ResetStateCounter();

            var nfa = new NFA('a');

            var nfaMaybe = nfa.Maybe();

            Assert.AreEqual((uint)3, nfaMaybe.Initial);

            Assert.AreEqual((uint)4, nfaMaybe.Final);

            Assert.IsTrue(nfaMaybe.Transitions.ContainsKey((nfa.Initial, 'a')));
            Assert.IsTrue(nfaMaybe.Transitions.ContainsKey((1, null)));
            Assert.IsTrue(nfaMaybe.Transitions.ContainsKey((2, null)));
        }

        [TestMethod]
        public void MultNFA(){
            NFA.ResetStateCounter();

            var nfa = new NFA('a');

            var nfaMult = nfa.Mult();

            Assert.AreEqual((uint)0, nfaMult.Initial);

            Assert.AreEqual((uint)0, nfaMult.Final);

            Assert.IsTrue(nfaMult.Transitions.ContainsKey((nfa.Initial, 'a')));
            Assert.IsTrue(nfaMult.Transitions[(0, 'a')].Contains(1));
            Assert.IsTrue(nfaMult.Transitions[(1, null)].Contains(0));            
        }

        [TestMethod]
        public void PlusNFA(){
            NFA.ResetStateCounter();

            var nfa = new NFA('a');

            var nfaMult = nfa.Mult();

            Assert.AreEqual((uint)0, nfaMult.Initial);

            Assert.AreEqual((uint)0, nfaMult.Final);

            Assert.IsTrue(nfaMult.Transitions.ContainsKey((nfa.Initial, 'a')));
            Assert.IsTrue(nfaMult.Transitions[(0, 'a')].Contains(1));
            Assert.IsTrue(nfaMult.Transitions[(1, null)].Contains(0));            
        }

        
    }
}
