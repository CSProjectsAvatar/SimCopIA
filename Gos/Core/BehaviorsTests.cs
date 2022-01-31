using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;

namespace Core {
    [TestClass]
    public class BehaviorsTests {

        [TestInitialize]
        public void Init() {

        }
        
        
        [TestMethod]
        public void WorkerBehavTest_1() {
            var worker = BehaviorsLib.Worker;
            var server1 = new Server("S1");
            var server2 = new Server("S2");
            
            // server2.AddLayer();
            var r1 = new Resource("img1");
            var r2 = new Resource("img2");
            var r3 = new Resource("index");

            server2.Stats.availableResources.AddRange(new[] { r1, r2 });

            var p1 = new Request("S1", "S2", RequestType.AskSomething);
            p1.AskingRscs.AddRange(new[] { r1 });

            var p2 = new Request("S1", "S2", RequestType.AskSomething);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            var p3 = new Request("S1", "S2", RequestType.AskSomething);
            p3.AskingRscs.AddRange(new[] { r1, r2, r3 });

            server2.HandlePerception(p1);


            // worker.Run(server1.Stats, p);
        }
        [TestMethod]
        public void FilterResourcesTest() {
            var worker = BehaviorsLib.Worker;
            var server1 = new Server("S1");
            var server2 = new Server("S2");
            
            // server2.AddLayer();
            var r1 = new Resource("img1");
            var r2 = new Resource("img2");
            var r3 = new Resource("index");

            server2.Stats.availableResources.AddRange(new[] { r1, r2 });

            var p1 = new Request("S1", "S2", RequestType.AskSomething);
            p1.AskingRscs.AddRange(new[] { r1 });

            var p2 = new Request("S1", "S2", RequestType.AskSomething);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            var p3 = new Request("S1", "S2", RequestType.AskSomething);
            p3.AskingRscs.AddRange(new[] { r1, r2, r3 });

            server2.HandlePerception(p1);


            // worker.Run(server1.Stats, p);
        }
        [TestMethod]
        public void ContractorBehavTest_1() {
           
        }

    }

}
