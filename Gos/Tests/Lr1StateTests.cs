using Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class Lr1StateTests : CompilerTests {
        [TestMethod]
        public void Equality() {
            var i1 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i2 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i3 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 2);
            var i4 = new Lr1Item(E > X, Token.TypeEnum.LPar, 0);

            Assert.AreEqual(new Lr1State(new[] { i1, i3 }), new Lr1State(new[] { i3, i2 }));
            Assert.AreNotEqual(new Lr1State(new[] { i1, i2 }), new Lr1State(new[] { i3 }));
            Assert.AreEqual(new Lr1State(new[] { i1, i2 }), new Lr1State(new[] { i1 }));
        }

        [TestMethod]
        public void InsideHashSet() {
            var i1 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i2 = new Lr1Item(E > X + Y, Token.TypeEnum.LPar, 1);
            var i3 = new Lr1Item(E > X, Token.TypeEnum.RPar, 0);
            var set = new HashSet<Lr1State>();
            set.Add(new Lr1State(new[] { i1, i3 }));
            set.Add(new Lr1State(new[] { i3, i2 }));

            Assert.AreEqual(1, set.Count);
            Assert.IsTrue(set.Contains(new Lr1State(new[] { i2, i3 })));

            var s2 = new HashSet<Lr1State>();
            s2.Add(new Lr1State(new[] { i2, i3 }));

            Assert.IsTrue(set.IsSubsetOf(s2));
            Assert.IsTrue(s2.IsSubsetOf(set));
        }
    }
}
