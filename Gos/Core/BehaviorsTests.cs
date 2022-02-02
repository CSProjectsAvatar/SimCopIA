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
        private Server s3;

        private Resource r1;
        private Resource r2;
        private Resource r3;
        private Request p1;
        private Request p2;
        private Request p3;
        private Request p4;
        private Request p5;
        private Request p6;
        private Behavior falenLeader;
        private List<Server> servers;
        private Layer layer;
        #endregion

        [TestInitialize]
        public void Init() {
            s1 = new Server("S1");
            s2 = new Server("S2");
            s3 = new Server("S2");

            r1 = new Resource("img1");
            r2 = new Resource("img2");
            r3 = new Resource("index");

            p1 = new Request("S1", "S2", ReqType.Asking);
            p1.AskingRscs.AddRange(new[] { r1 });

            p2 = new Request("S3", "S2", ReqType.Asking);
            p3 = new Request("S3", "S2", ReqType.Asking);
            p4 = new Request("S3", "S2", ReqType.Asking);
            p5 = new Request("S3", "S2", ReqType.Asking);
            p6 = new Request("S3", "S2", ReqType.Asking);


            servers = new List<Server> { s1, s2 };
            layer = new Layer();

            env = new Env();
        }
        
        
        
        [TestMethod]
        public void WorkerBehavTest_1() {
            
            // server2.HandlePerception(p1);


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

            var p2 = new Request("S1", "S2", ReqType.Asking);
            p2.AskingRscs.AddRange(new[] { r1, r2, r3 });

            var p3 = new Request("S1", "S2", ReqType.DoIt);

            server2.Stats.AvailableResources.Add(r1);
            server2.Stats.AvailableResources.Add(r3);

            contractor.Run(server2.Stats, p2);
            contractor.Run(server2.Stats, p3);

        }

        #region FalenLeader

        [TestMethod]
        public void FalenLeaderBehavTest_1()
        {
            falenLeader = BehaviorsLib.FallenLeader;

            layer.behaviors.Add(falenLeader);

            s2.AddLayer(layer);
            s3.AddLayer(layer);
            s2.Stats.MicroService.ChangeLeader( "S1");

            env.AddServerList(servers);

            //2do if sin convertirse en lider
            env.SubsribeEvent(10, p2);
            env.SubsribeEvent(18, p3);

            //1er if reiniciando los valores
            env.SubsribeEvent(34, p1);

            //al final se convierte en jefe
            env.SubsribeEvent(42, p4);
            env.SubsribeEvent(56, p5);
            env.SubsribeEvent(67, p5);
            env.SubsribeEvent(110, p6);

            env.Run();

            ///falenLeader.Run(server2.Stats, p2);

        }
        #endregion



    }
}
