using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;

namespace Core {
    [TestClass]
    public class BehaviorsTests : BaseTest {

        [TestInitialize]
        public void Init() {

        }

        [TestMethod]
        public void WorkerBehavTest_1() {
           
        }

        [TestMethod]
        public void ContractorBehavTest_1() {
            var contractor = BehaviorsLib.Contractor;

            // ...

            // Assert.AreEqual(0, algo);
        }

    }
}
