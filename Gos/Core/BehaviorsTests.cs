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
            var worker = BehaviorsLib.Worker;
            var server1 = new Server("S1");
            var server2 = new Server("S2");
            
            var r1 = new Resource("img1");
            var r2 = new Resource("index.html");
            
            var p = new Request("S1", "S2", RequestType.AskSomething);
            p.Asking.AddRange(new[] { r1, r2 });


            worker.Run(server.Stats, p);
        }

        [TestMethod]
        public void ContractorBehavTest_1() {
            var contractor = BehaviorsLib.Contractor;

            // ...

            // Assert.AreEqual(0, algo);
        }

    }
}
