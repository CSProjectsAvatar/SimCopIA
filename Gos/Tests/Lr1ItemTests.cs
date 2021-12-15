using Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class Lr1ItemTests : CompilerTests {
        [TestMethod]
        public void Equality() {
            var i1 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i2 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i3 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 2);
            var i4 = new Lr1Item(E > X, Token.TypeEnum.LPar, 0);

            Assert.AreEqual(i1, i2);
            Assert.AreNotEqual(i1, i3);
            Assert.AreNotEqual(i2, i3);
            Assert.AreNotEqual(i4, i3);
        }

        [TestMethod]
        public void InsideHashSets() {
            var i1 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i2 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var set = new HashSet<Lr1Item>();
            set.Add(i1);
            set.Add(i2);

            Assert.AreEqual(1, set.Count);

            var s2 = new HashSet<Lr1Item>();
            s2.Add(i2);

            Assert.IsTrue(set.IsSubsetOf(s2));
            Assert.IsTrue(s2.IsSubsetOf(set));
        }
    }
}
