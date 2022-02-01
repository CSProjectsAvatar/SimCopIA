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

            //server2.Stats.availableResources.AddRange(new[] { r1, r2 });

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

            var e = new Env();
            var contractor = BehaviorsLib.Contractor;

            var server1 = new Server("S1");
            var server2 = new Server("S2");

            
            var r1 = new Resource("img1");
            var r2 = new Resource("img2");
            var r3 = new Resource("index");

            var p2 = new Request("S1", "S2", RequestType.AskSomething);
            p2.AskingRscs.AddRange(new[] { r1, r2, r3 });

            var p3 = new Request("S1", "S2", RequestType.DoSomething);

            server2.Stats.AvailableResources.Add(r1);
            server2.Stats.AvailableResources.Add(r3);

            contractor.Run(server2.Stats, p2);
            contractor.Run(server2.Stats, p3);

        }

        #region FalenLeader
        #region  vars
        private Server s1;
        private Server s2;
        private Behavior falenLeader;
        private Request p1;
        private Request p2;
        private Env env;
        private List<Server> servers;
        private Layer layer;
        #endregion
        #endregion


        [TestInitialize]
        public void Init1()
        {
            s1 = new Server("S1");
            s2 = new Server("S2");

            env = new Env();
            falenLeader = BehaviorsLib.FalenLeader;
            layer = new Layer();

            p1 = new Request("S1", "S2", RequestType.AskSomething);
            p2 = new Request("S1", "S2", RequestType.Ping);

            servers = new List<Server> { s1, s2 };
        }


        [TestMethod]
        public void FalenLeaderBehavTest_1()
        {
            layer.behaviors.Add(falenLeader);

            s2.AddLayer(layer);
            s2.Stats.MicroService.ChangeLeader( "S1");

            env.AddServerList(servers);


            env.SubsribeEvent(10, p1);
            env.Run();

            ///falenLeader.Run(server2.Stats, p2);


        }

    }
}
