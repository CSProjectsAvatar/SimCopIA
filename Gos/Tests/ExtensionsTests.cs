using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class ExtensionsTests {
        [DataTestMethod]
        [DataRow(1.5, 1.5, true)]
        [DataRow(1.3, 1.2, false)]
        [DataRow(double.MaxValue, double.MaxValue, true)]
        [DataRow(double.MinValue, double.MinValue, true)]
        [DataRow(Math.PI, Math.PI, true)]
        [DataRow(Math.PI, Math.PI + 1e-8, false)]
        public void Eq(double x, double y, bool expected) {
            Assert.AreEqual(expected, x.Eq(y));
        }
    }
}
