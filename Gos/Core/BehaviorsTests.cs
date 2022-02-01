using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;

namespace Core {
    [TestClass]
    public class BehaviorsTests {
        #region  vars
        private Env env;
        private Server s1;
        private Server s2;
        private Resource r1;
        private Resource r2;
        private Resource r3;
        private Request p1;
        private Request p2;
        private Request p3;
        #endregion

        [TestInitialize]
        public void Init() {
            s1 = new Server("S1");
            s2 = new Server("S2");
            
            // server2.AddLayer();
            r1 = new Resource("img1");
            r2 = new Resource("img2");
            r3 = new Resource("index");

            p1 = new Request("S1", "S2", RequestType.AskSomething);
            p1.AskingRscs.AddRange(new[] { r1 });

            p2 = new Request("S1", "S2", RequestType.AskSomething);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            p3 = new Request("S1", "S2", RequestType.AskSomething);
            p3.AskingRscs.AddRange(new[] { r1, r2, r3 });

            env = new Env();
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

            server2.Stats.AvailableResources.AddRange(new[] { r1, r2 });

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

            server2.Stats.AvailableResources.AddRange(new[] { r1, r2 });

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
