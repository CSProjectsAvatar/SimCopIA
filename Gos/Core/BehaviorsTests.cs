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

        [TestMethod]
        public void FalenLeaderBehavTest_1()
        {
            var e = new Env();
            var falenLeader = BehaviorsLib.FalenLeader;

            var server1 = new Server("S1");
            var server2 = new Server("S2");

            Layer l = new Layer();

            l.behaviors.Add(falenLeader);
            server2.AddLayer(l);

            server2.Stats.MicroService.ChangeLeader( "S1");
            List<Server> s = new List<Server> { server1, server2 };

            e.AddServerList(s);

            //Dictionary<string, bool> data = new  Dictionary<string, bool>{ };
            var p1 = new Request("S1", "S2", RequestType.AskSomething);

            var p2 = new Request("S1", "S2", RequestType.Ping);

            e.SubsribeEvent(10, p1);
            e.Run();

            ///falenLeader.Run(server2.Stats, p2);


        }

    }
}
