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

        private void ResetStatesCounter(){
            NFA.ResetStateCounter();
            DFA.ResetStateCounter();
            DFAState.ResetStateCounter();

        }
        
        // Test que prueba la clausura del primer estado de un NFA
        [TestMethod]
        public void ClousureFirstNFAState() {
            ResetStatesCounter();

            var nfa = new NFA('a');

            var dfa = new ConverterToDFA(nfa).eClosure(nfa.Initial);

            Assert.AreEqual(1, dfa.Count);
            Assert.IsTrue(dfa.Contains(nfa.Initial));
        }

         // Test que prueba la clausura del primer estado de un NFA Union
        [TestMethod]
        public void ClousureUnionFirstNFAState() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            var firstClousure = new ConverterToDFA(nfaUnion).eClosure(nfaUnion.Initial);

            Assert.AreEqual(3, firstClousure.Count);
            Assert.IsTrue(firstClousure.Contains(nfa1.Initial));
            Assert.IsTrue(firstClousure.Contains(nfa2.Initial));
            
        }

        // Test que prueba la clausura del primer estado de un NFA Multiplicacion
        [TestMethod]
        public void ClousureMultiplicationFirstNFAState() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');

            var firstClnfaMult = nfa1.Mult();

            var firstClousure = new ConverterToDFA(firstClnfaMult).eClosure(firstClnfaMult.Initial);

            Assert.AreEqual(1, firstClousure.Count);
            Assert.IsTrue(firstClousure.Contains(nfa1.Initial));
        }
        
        // Test que prueba la clausura del primer estado de un NFA Maybe
        [TestMethod]
        public void ClousureMaybeFirstNFAState() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');

            var firstClnfaMaybe = nfa1.Maybe();

            var firstClousure = new ConverterToDFA(firstClnfaMaybe).eClosure(firstClnfaMaybe.Initial);

            Assert.AreEqual(4, firstClousure.Count);
            Assert.IsTrue(firstClousure.Contains(nfa1.Initial));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 1));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 2));
            Assert.IsTrue(firstClousure.Contains(nfa1.Final + 3));
        }
        
        // Test que prueba el Goto del primer estado de un NFA Union
        [TestMethod]
        public void GotoUnionFirstNFAState() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            var firstClousure = new ConverterToDFA(nfaUnion).eClosure(nfaUnion.Initial); 

            var firstGoto = new ConverterToDFA(nfaUnion).GoTo(firstClousure, 'a');

            Assert.AreEqual(1, firstGoto.Count);
            Assert.IsTrue(firstGoto.Contains(nfa1.Final));

            var secondGoto = new ConverterToDFA(nfaUnion).GoTo(firstClousure, 'b');

            Assert.AreEqual(1, secondGoto.Count);
            Assert.IsTrue(secondGoto.Contains(nfa2.Final));
        }
    
        // Test para probar el StringHash de DFAState para usar el HashSet
        [TestMethod]
        public void GetHashTest(){
            ResetStatesCounter();

            var arr = new uint [] { 1, 2, 3};
            var arr2 = new uint [] { 1, 5, 3};
            var arr3 = new uint [] { 1, 2, 3, 8};
            var Q0 = new DFAState(arr);
            var Q1 = new DFAState(arr);
            var Q2 = new DFAState(arr2);
            var Q3 = new DFAState(arr3);

            Assert.AreEqual(Q0.StringHash(), Q1.StringHash());
            Assert.AreNotEqual(Q0.StringHash(), Q2.StringHash());
            Assert.AreNotEqual(Q0.StringHash(), Q3.StringHash());

            var set = new HashSet<string>();
            set.Add(Q0.StringHash());
            set.Add(Q1.StringHash());
            set.Add(Q2.StringHash());
            set.Add(Q3.StringHash());

            Assert.AreEqual(3, set.Count);
        }

        // Test de ToDFA para un NFA con un solo estado
        [TestMethod]
        public void MarkFinalStatesA() {
            ResetStatesCounter();

            var nfa = new NFA('a');
            
            var dfa = new ConverterToDFA(nfa).ToDFA();

            Assert.AreEqual(1, dfa.FinalStates.Count);

            var final1 = dfa.States.Where(q => q.Contains(nfa.Final)).First().StNumber;
            Assert.IsTrue(dfa.FinalStates.Any(q => q == final1));
        }
   
        // Test de ToDFA para probar estados de NFA Union
        [TestMethod]
        public void ToDFATestUnion() {
            ResetStatesCounter();
            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            var dfa = new ConverterToDFA(nfaUnion).ToDFA();

            Assert.AreEqual(3, dfa.States.Count);
            Assert.AreEqual(2, dfa.FinalStates.Count);

            var initial = dfa.States.Where(q => q.Contains(nfa1.Initial) && q.Contains(nfa2.Initial))
                            .First().StNumber;
            Assert.AreEqual(initial, dfa.Initial);
            
            var final1 = dfa.States.Where(q => q.Contains(nfa1.Final)).First().StNumber;
            Assert.IsTrue(dfa.FinalStates.Any(q => q == final1));

            var final2 = dfa.States.Where(q => q.Contains(nfa2.Final)).First().StNumber;
            Assert.IsTrue(dfa.FinalStates.Any(q => q == final2));
        }

        // Test de ToDFA probando Accept del DFA para un NFA con un solo estado
        [TestMethod]
        public void ToDFATestAccept() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('b');

            var nfaUnion = nfa1.Union(nfa2);

            var dfa = new ConverterToDFA(nfaUnion).ToDFA();

            Assert.IsTrue(dfa.Accept("a"));
            Assert.IsTrue(dfa.Accept("b"));
            Assert.IsFalse(dfa.Accept("ab"));
            Assert.IsFalse(dfa.Accept("ba"));
            Assert.IsFalse(dfa.Accept(""));
            Assert.IsFalse(dfa.Accept("c"));
        }
    
        // Test de ToDFA probando Accept del DFA para un NFA con varios estados
        [TestMethod]
        public void ToDFATestAccept2() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var dfa = new ConverterToDFA(nfa1.Concat(nfa2).Concat(nfa3)).ToDFA();

            Assert.IsTrue(dfa.Accept("ala"));
            Assert.IsFalse(dfa.Accept("al"));
        }

        // Test de ToDFA probando Accept del DFA para un NFA con varios estados y union
        [TestMethod]
        public void ToDFATestAcceptConcat() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);

            var nfa4 = new NFA('c');
            var nfa5 = new NFA('o');
            var nfa6 = new NFA('l');

            var nfaC2 = nfa4.Concat(nfa5).Concat(nfa6);

            var dfa = new ConverterToDFA(nfaC.Union(nfaC2)).ToDFA();

            Assert.IsTrue(dfa.Accept("ala"));
            Assert.IsTrue(dfa.Accept("col"));

            Assert.IsFalse(dfa.Accept("al"));
            Assert.IsFalse(dfa.Accept("cola"));
            Assert.IsFalse(dfa.Accept("colo"));
            Assert.IsFalse(dfa.Accept("colol"));
            Assert.IsFalse(dfa.Accept("co"));
            Assert.IsFalse(dfa.Accept("bobo"));
        }
    
        // Test de ToDFA probando Accept del DFA para un string con *
        [TestMethod]
        public void ToDFATestAcceptConcatMult() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            nfa2.ToString();
            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);
            var nfaMult = nfaC.Mult();

            var dfa = new ConverterToDFA(nfaMult).ToDFA();

            Assert.IsTrue(dfa.Accept("ala"));
            Assert.IsTrue(dfa.Accept(""));
            Assert.IsTrue(dfa.Accept("alaala"));
            Assert.IsTrue(dfa.Accept("alaalaala"));

            Assert.IsFalse(dfa.Accept("al"));
            Assert.IsFalse(dfa.Accept("colol"));
            Assert.IsFalse(dfa.Accept("co"));
            Assert.IsFalse(dfa.Accept("bobo"));
            Assert.IsFalse(dfa.Accept("alala"));
        }
    

        // Test de ToDFA probando Accept del DFA para un string union con *
        [TestMethod]
        public void ToDFATestAcceptConcatUnionMult() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);

            var nfa4 = new NFA('c');
            var nfa5 = new NFA('o');
            var nfa6 = new NFA('l');

            var nfaC2 = nfa4.Concat(nfa5).Concat(nfa6);

            var nfaUnion = nfaC.Union(nfaC2);
            var nfaMult = nfaUnion.Mult();

            var dfa = new ConverterToDFA(nfaMult).ToDFA();

            Assert.IsTrue(dfa.Accept("ala"));
            Assert.IsTrue(dfa.Accept("col"));
            Assert.IsTrue(dfa.Accept(""));
            Assert.IsTrue(dfa.Accept("alaala"));
            Assert.IsTrue(dfa.Accept("colcol"));
            Assert.IsTrue(dfa.Accept("alacol"));
            Assert.IsTrue(dfa.Accept("alacolala"));

            Assert.IsFalse(dfa.Accept("al"));
            Assert.IsFalse(dfa.Accept("cola"));
            Assert.IsFalse(dfa.Accept("colo"));
            Assert.IsFalse(dfa.Accept("colol"));
            Assert.IsFalse(dfa.Accept("co"));
            Assert.IsFalse(dfa.Accept("bobo"));
            Assert.IsFalse(dfa.Accept("alala"));
        }
    
        // Test de ToDFA probando Accept del DFA para un string concat con ?
        [TestMethod]
        public void ToDFATestAcceptConcatMaybe() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);
            var nfaM = nfaC.Maybe(); // (ala)?

            var dfa = new ConverterToDFA(nfaM).ToDFA();

            Assert.IsTrue(dfa.Accept("ala"));
            Assert.IsTrue(dfa.Accept(""));

            Assert.IsFalse(dfa.Accept("al"));
            Assert.IsFalse(dfa.Accept("cola"));
            Assert.IsFalse(dfa.Accept("colo"));
            Assert.IsFalse(dfa.Accept("colol"));
            Assert.IsFalse(dfa.Accept("co"));
            Assert.IsFalse(dfa.Accept("bobo"));
            Assert.IsFalse(dfa.Accept("alala"));
        }
    
        // Test de ToDFA probando Accept del DFA para un string concat ? y un char
        [TestMethod]
        public void AcceptConcatMaybeChar() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');

            var nfaMaybe = nfa1.Maybe(); // a?

            var nfa4 = new NFA('c');

            var nfaConcat = nfaMaybe.Concat(nfa4); // a?c

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("c"));
            Assert.IsTrue(dfa.Accept("ac"));

            Assert.IsFalse(dfa.Accept("aa"));
            Assert.IsFalse(dfa.Accept("aac"));
            Assert.IsFalse(dfa.Accept("cc"));
            Assert.IsFalse(dfa.Accept("acc"));
            Assert.IsFalse(dfa.Accept("aaaaa"));
        }
    

        // Test de ToDFA probando Accept del DFA para un string concat ? y un char
        [TestMethod]
        public void ToDFATestAcceptConcatMaybeChar() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);
            var nfaMaybe = nfaC.Maybe(); // (ala)?

            var nfa4 = new NFA('c');

            var nfaConcat = nfaMaybe.Concat(nfa4); // (ala)?c

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("c"));
            Assert.IsTrue(dfa.Accept("alac"));

            Assert.IsFalse(dfa.Accept("alc"));
            Assert.IsFalse(dfa.Accept("ac"));
            Assert.IsFalse(dfa.Accept("alacc"));
            Assert.IsFalse(dfa.Accept("lac"));
            Assert.IsFalse(dfa.Accept("alaalac"));
            Assert.IsFalse(dfa.Accept("al"));
            Assert.IsFalse(dfa.Accept("cola"));
            Assert.IsFalse(dfa.Accept("alala"));
        }
    
        // Test de ToDFA probando Accept del DFA para un string concat ? y *
        [TestMethod]
        public void AcceptConcatMaybeAndMultChar() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfaMaybe = nfa1.Maybe(); // a?

            var nfa4 = new NFA('c');
            var nfaMult = nfa4.Mult(); // c*

            var nfaConcat = nfaMaybe.Concat(nfaMult); // a?c*

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("ac"));
            Assert.IsTrue(dfa.Accept("a"));
            Assert.IsTrue(dfa.Accept("c"));
            Assert.IsTrue(dfa.Accept(""));
            Assert.IsTrue(dfa.Accept("cc"));
            Assert.IsTrue(dfa.Accept("acc"));

            Assert.IsFalse(dfa.Accept("aca"));
            Assert.IsFalse(dfa.Accept("aa"));
            Assert.IsFalse(dfa.Accept("ab"));
            Assert.IsFalse(dfa.Accept("ca"));
            Assert.IsFalse(dfa.Accept("caa"));
            Assert.IsFalse(dfa.Accept("cco"));
        }
    
        // Test de ToDFA probando Accept del DFA para un string concat ? y *
        [TestMethod]
        public void ToDFATestAcceptConcatMaybeAndMult() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);
            var nfaMaybe = nfaC.Maybe(); // (ala)?

            var nfa4 = new NFA('c');
            var nfa5 = new NFA('o');
            var nfa6 = new NFA('l');

            var nfaC2 = nfa4.Concat(nfa5).Concat(nfa6);
            var nfaMult = nfaC2.Mult(); // (col)*

            var nfaConcat = nfaMaybe.Concat(nfaMult); // (ala)?(col)*

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("alacol"));
            Assert.IsTrue(dfa.Accept("ala"));
            Assert.IsTrue(dfa.Accept("col"));
            Assert.IsTrue(dfa.Accept(""));
            Assert.IsTrue(dfa.Accept("colcol"));
            Assert.IsTrue(dfa.Accept("alacolcol"));

            Assert.IsFalse(dfa.Accept("alacolala"));
            Assert.IsFalse(dfa.Accept("alaala"));
            Assert.IsFalse(dfa.Accept("al"));
            Assert.IsFalse(dfa.Accept("cola"));
            Assert.IsFalse(dfa.Accept("colo"));
            Assert.IsFalse(dfa.Accept("colol"));
            Assert.IsFalse(dfa.Accept("co"));
            Assert.IsFalse(dfa.Accept("bobo"));
            Assert.IsFalse(dfa.Accept("alala"));
        }
    
    
        // Test de ToDFA probando Accept del DFA con union, concat, ?, * y +
        [TestMethod]
        public void AcceptConcatMaybeConcatPlusOne() {
            ResetStatesCounter();

            var nfaA = new NFA('a');
            var nfaMaybe = nfaA.Maybe(); // a?

            var nfaC = new NFA('c');
            var nfaMult = nfaC.Mult(); // c*

            var nfaP = new NFA('p');
            var nfaPlus = nfaP.Plus(); // p+

            var nfaConcat = nfaMaybe.Concat(nfaPlus).Concat(nfaMult); // a?p+c*

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("apc"));
            Assert.IsTrue(dfa.Accept("ap"));
            Assert.IsTrue(dfa.Accept("pc"));
            Assert.IsTrue(dfa.Accept("p"));
            Assert.IsTrue(dfa.Accept("pcc"));
            Assert.IsTrue(dfa.Accept("appppcccc"));

            Assert.IsFalse(dfa.Accept("aapc"));
            Assert.IsFalse(dfa.Accept("aapp"));
            Assert.IsFalse(dfa.Accept("apca"));
            Assert.IsFalse(dfa.Accept("pca"));
            Assert.IsFalse(dfa.Accept("ppca"));
            Assert.IsFalse(dfa.Accept("ccpp"));
            Assert.IsFalse(dfa.Accept("aa"));
        }
    

        // Test de ToDFA probando Accept del DFA con union, concat, ?, * y +
        [TestMethod]
        public void AcceptConcatMaybePlusMult() {
            ResetStatesCounter();

            var nfaA = new NFA('a');
            var nfaMaybe = nfaA.Maybe(); // a?

            var nfaC = new NFA('c');
            var nfaMult = nfaC.Mult(); // c*

            var nfaP = new NFA('p');
            var nfaP2 = new NFA('i');
            var nfaPConcat = nfaP.Concat(nfaP2);
            var nfaPlus = nfaPConcat.Plus(); // (pi)+

            var nfaConcat = nfaMaybe.Concat(nfaPlus).Concat(nfaMult); // a?(pi)+c*

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("apic"));
            Assert.IsTrue(dfa.Accept("api"));
            Assert.IsTrue(dfa.Accept("pic"));
            Assert.IsTrue(dfa.Accept("pi"));
            Assert.IsTrue(dfa.Accept("picc"));
            Assert.IsTrue(dfa.Accept("apipipipicccc"));

            Assert.IsFalse(dfa.Accept("aapic"));
            Assert.IsFalse(dfa.Accept("aapipi"));
            Assert.IsFalse(dfa.Accept("apica"));
            Assert.IsFalse(dfa.Accept("pica"));
            Assert.IsFalse(dfa.Accept("pipica"));
            Assert.IsFalse(dfa.Accept("ccpipi"));
            Assert.IsFalse(dfa.Accept("aa"));
        }
    

        // Test de ToDFA probando Accept del DFA con union, concat, ?, * y +
        [TestMethod]
        public void AcceptConcatMaybeConcatPlus() {
            ResetStatesCounter();

            var nfa1 = new NFA('a');
            var nfa2 = new NFA('l');
            var nfa3 = new NFA('a');

            var nfaC = nfa1.Concat(nfa2).Concat(nfa3);
            var nfaMaybe = nfaC.Maybe(); // (ala)?

            var nfa4 = new NFA('c');
            var nfa5 = new NFA('o');
            var nfa6 = new NFA('l');

            var nfaC2 = nfa4.Concat(nfa5).Concat(nfa6);
            var nfaMult = nfaC2.Mult(); // (col)*

            var nfa7 = new NFA('p');
            var nfa8 = new NFA('i');

            var nfaPi = nfa7.Concat(nfa8);
            var nfaPlus = nfaPi.Plus(); // (pi)+

            var nfaConcat = nfaMaybe.Concat(nfaPlus).Concat(nfaMult); // (ala)?(pi)+(col)*

            var dfa = new ConverterToDFA(nfaConcat).ToDFA();

            Assert.IsTrue(dfa.Accept("alapicol"));
            Assert.IsTrue(dfa.Accept("alapi"));
            Assert.IsTrue(dfa.Accept("picol"));
            Assert.IsTrue(dfa.Accept("pi"));
            Assert.IsTrue(dfa.Accept("pipi"));
            Assert.IsTrue(dfa.Accept("pipicol"));
            Assert.IsTrue(dfa.Accept("pipicolcol"));
            Assert.IsTrue(dfa.Accept("picolcol"));
            Assert.IsTrue(dfa.Accept("alapipipicolcolcolcol"));

            Assert.IsFalse(dfa.Accept("alaalapicolala"));
            Assert.IsFalse(dfa.Accept("alaalapipi"));
            Assert.IsFalse(dfa.Accept("alpi"));
            Assert.IsFalse(dfa.Accept("picolala"));
            Assert.IsFalse(dfa.Accept("pipicola"));
            Assert.IsFalse(dfa.Accept("cololpipi"));
            Assert.IsFalse(dfa.Accept("pico"));
            Assert.IsFalse(dfa.Accept("alaala"));
        }
    
    
    
    
    }

}
