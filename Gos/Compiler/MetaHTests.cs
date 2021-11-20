using Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace Compiler {
    [TestClass]
    class MetaHTests {
        
        [TestMethod]
        public static void RunMeta(){
            var meta = new MetaHeuristics();
            var list = Individual.Sampler(30);
            meta.Run( list,
             (Individual i) => { return Math.Abs(i.Doers - 57); },
             (Individual i) => { return 0 < i.MonthlyMaintennanceCost ; },
             1000);

            Assert.AreEqual(57, list[0].Doers);
        }
   }
}
