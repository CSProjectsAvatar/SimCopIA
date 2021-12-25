using Compiler;
using Compiler.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Compiler.Token;

namespace Tests {
    [TestClass]
    public class LabelsTest : CompilerTests {

        public static void ResetStatesCounter(){
            NFA.ResetStateCounter();
            DFA.ResetStateCounter();
            DFAState.ResetStateCounter();

        }
        
       
        // Test de ToDFA probando que reconozca La etiqueta despes de concatenar
        [TestMethod]
        public void LabelsCheck() {
            ResetStatesCounter();

            var nfa1 = new NFA('l');
            var nfa2 = new NFA('e');
            var nfa3 = new NFA('t');

            var nfaLet = nfa1.Concat(nfa2).Concat(nfa3);
            nfaLet.SetLabel(TypeEnum.Let);

            var dfa = new ConverterToDFA(nfaLet).ToDFA();

            Assert.IsTrue(dfa.Accept("let", TypeEnum.Let));
        }

        // Test de ToDFA probando que reconozca La etiqueta despes de concat y unir
        [TestMethod]
        public void LabelsConcatUnion() {
            ResetStatesCounter();

            var nfa1 = new NFA('l');
            var nfa2 = new NFA('e');
            var nfa3 = new NFA('t');

            var nfaLet = nfa1.Concat(nfa2).Concat(nfa3);
            nfaLet.SetLabel(TypeEnum.Let);

            var nfa4 = new NFA('i');
            var nfa5 = new NFA('f');

            var nfaIf = nfa4.Concat(nfa5);
            nfaIf.SetLabel(TypeEnum.If);

            var nfaUnion = nfaLet.Union(nfaIf);

            var dfa = new ConverterToDFA(nfaUnion).ToDFA();

            Assert.IsTrue(dfa.Accept("let", TypeEnum.Let));
            Assert.IsTrue(dfa.Accept("if", TypeEnum.If));

            Assert.IsFalse(dfa.Accept("let", TypeEnum.If));
            Assert.IsFalse(dfa.Accept("if", TypeEnum.Let));
            Assert.IsFalse(dfa.Accept("ifl", TypeEnum.Let));
            Assert.IsFalse(dfa.Accept("i", TypeEnum.If));
            Assert.IsFalse(dfa.Accept("le", TypeEnum.Let));
            Assert.IsFalse(dfa.Accept("lets", TypeEnum.Let));
        }
    
         // Test de ToDFA probando que reconozca Label Ids despes de concat y unir
        [TestMethod]
        public void ToDFATestAcceptConcat() {
            ResetStatesCounter();

            var nfaLettreI = new NFA('i');
            var nfaLettreF = new NFA('f');
            var nfaLettreIA = nfaLettreI.Mult();
            var nfaLettreFA = nfaLettreF.Mult();

            var nfaID = nfaLettreIA.Concat(nfaLettreFA);
            nfaID.SetLabel(TypeEnum.Id);

            var nfaLettreI2 = new NFA('i');
            var nfaLettreF2 = new NFA('f');
            var nfaIf = nfaLettreI2.Concat(nfaLettreF2);
            nfaIf.SetLabel(TypeEnum.If);

            var nfaUnion = nfaID.Union(nfaIf);

            var dfa = new ConverterToDFA(nfaUnion).ToDFA();

            Assert.IsTrue(dfa.Accept("if", TypeEnum.If));
            Assert.IsTrue(dfa.Accept("iiii", TypeEnum.Id));
            Assert.IsTrue(dfa.Accept("iiiif", TypeEnum.Id));
            Assert.IsTrue(dfa.Accept("f", TypeEnum.Id));
            Assert.IsTrue(dfa.Accept("iff", TypeEnum.Id));
            Assert.IsTrue(dfa.Accept("ifff", TypeEnum.Id));

            Assert.IsFalse(dfa.Accept("let", TypeEnum.If));
            Assert.IsFalse(dfa.Accept("i", TypeEnum.If));
        }
    
    }

}
