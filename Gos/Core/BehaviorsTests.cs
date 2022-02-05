using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;


namespace Core {
    [TestClass]
    public class BehaviorsTests :BaseTest{
        #region  vars
        private Env env;
        private Server s1;
        private Server s2;
        private Server s3;
        private Server s4;
        private Layer workerL;
        private Resource r1;
        private Resource r2;
        private Resource r3;
        private Request p1;

        private Request p2;
        private Request p3;
        private Request p4;
        private Request p4_1;

        private Request p5;
        private Request p5_1;

        private Request p6;
        private Request p6_1;

        private Request p7;
        private Request p8;

        private Response res1;

        private Request p1_1;
        private Request p2_1;


        private Behavior workerB;
        private Behavior falenLeader;


        private Behavior contractor;
        private Behavior falenLeader2;

        private List<Server> servers;
        private Layer fallenL;
        private Layer fallenL2;


        private ILogger<Server> loggerServer;
        private ILogger<Status> loggerStatus ;
        private ILogger<MicroService> loggerMS;
        private ILogger<Env> loggerEnv;

        private List<Behavior> listBehavior;
        #endregion

        [TestInitialize]
        public void Init() {

            loggerServer = LoggerFact.CreateLogger<Server>();
            loggerStatus = LoggerFact.CreateLogger<Status>();
            loggerMS = LoggerFact.CreateLogger<MicroService>();
            loggerEnv = LoggerFact.CreateLogger<Env>();

            s1 = new Server("S1", loggerServer, loggerStatus);
            s2 = new Server("S2", loggerServer, loggerStatus);
            s3 = new Server("S3", loggerServer, loggerStatus);
            s4 = new Server("S4", loggerServer, loggerStatus);

            workerL = new Layer();
            workerB = BehaviorsLib.Worker;
            workerL.behaviors.Add(workerB);

            contractor = BehaviorsLib.Contractor;
            falenLeader = BehaviorsLib.FallenLeader;

            fallenL = new Layer();
            fallenL.behaviors.Add(falenLeader);
            fallenL.behaviors.Add(workerB);
            fallenL.behaviors.Add(contractor);


            falenLeader2 = BehaviorsLib.FallenLeader;
            fallenL2 = new Layer();
            fallenL2.behaviors.Add(falenLeader2);

            listBehavior = new List<Behavior> {falenLeader, contractor};

            r1 = new Resource("img1");
            r2 = new Resource("img2");
            r3 = new Resource("index");

            p1 = new Request("S1", "S2", ReqType.Asking);
            p1.AskingRscs.AddRange(new[] { r1, r2, r3 });
            p1_1 = new Request("S1", "S3", ReqType.Asking);


            p2 = new Request("S3", "S2", ReqType.Asking);
            p2_1 = new Request("S2", "S3", ReqType.Asking);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            p3 = new Request("S3", "S2", ReqType.Asking);
            p4 = new Request("S3", "S2", ReqType.Asking);
            p4_1 = new Request("S2", "S3", ReqType.Asking);

            p5 = new Request("S3", "S2", ReqType.Asking);
            p5_1 = new Request("S2", "S3", ReqType.Asking);

            p6 = new Request("S3", "S2", ReqType.Asking);
            p6_1 = new Request("S4", "S3", ReqType.Asking);

            p7 = new Request("S1", "S2", ReqType.DoIt);
            p8 = new Request("S1", "S2", ReqType.Ping);

            res1 = new Response(7, "s1", "s2", ReqType.Asking, new Dictionary<string, bool> { });

            //p7 = new Request("S4", "S2", ReqType.Asking);
            p7.AskingRscs.AddRange(new[] { r1, r2, r3 });

            s1.SetResources(new List<Resource> { r1});
            s2.SetResources(new List<Resource> { r1, r2 });
            s3.SetResources(new List<Resource> { r1, r2, r3 });

            servers = new List<Server> { s1, s2, s3 };

            env = new Env(loggerEnv,loggerMS);
            env.AddServerList(servers);
        }
        [TestCleanup]
        public void Clean() {
            MicroService.Services.Clear();
            Resource.Resources.Clear();
        }
        
        
        [TestMethod]
        public void WorkerBehavTest_1() {
            var p1Do = new Request("S1", "S2", ReqType.DoIt);
            p1Do.AskingRscs.AddRange(new[] { r1 });
            var p2Do = new Request("S3", "S2", ReqType.DoIt);
            p2Do.AskingRscs.AddRange(new[] { r1, r2 });
            var p7Do = new Request("S4", "S2", ReqType.DoIt);
            p7Do.AskingRscs.AddRange(new[] { r1, r2, r3 });

            s2.AddLayer(workerL);
            s2.Stats.AcceptReq(p1Do);
            s2.HandlePerception(p1Do);

            s2.Stats.AcceptReq(p2Do);
            s2.HandlePerception(p2Do);

            s2.Stats.AcceptReq(p7Do);
            s2.HandlePerception(p7Do);

            env.Run();

            var respToS1 = s1.Stats.GetMsgBySender("S2");
            var respToS3 = s3.Stats.GetMsgBySender("S2");
            var respToS4 = s4.Stats.GetMsgBySender("S2");

            Assert.AreEqual(1, respToS1.Count);
            Assert.AreEqual(1, respToS3.Count);
            Assert.AreEqual(0, respToS4.Count);

            var dataS1 = respToS1.First() as Response;
            var dataS3 = respToS3.First() as Response;
            var dataS4 = respToS4.FirstOrDefault();
            
            Assert.AreEqual(1, dataS1.AnswerRscs.Count);
            Assert.IsTrue(dataS1.AnswerRscs[r1.Name]);

            Assert.AreEqual(2, dataS3.AnswerRscs.Count);
            Assert.IsTrue(dataS3.AnswerRscs[r1.Name]);
            Assert.IsTrue(dataS3.AnswerRscs[r2.Name]);
            
            Assert.AreEqual(null, dataS4);
        }

        [TestMethod]
        public void ContractorBehavTest_1() {

            var contractor = BehaviorsLib.Contractor;

            // TODO case Asking
            contractor.Run(s2.Stats, p1);
            Assert.AreEqual(s1.ID, s2.Stats.GetRequestSentToEnv().receiver);

            // TODO case DoIt
            contractor.Run(s2.Stats, p7);
            Assert.AreEqual(p7, s2.Stats.GetRequest());

            // TODO case Ping
            contractor.Run(s2.Stats, p8);
            Assert.AreEqual(s1.ID, s2.Stats.GetRequestSentToEnv().receiver);
        }

        #region FalenLeader

        [TestMethod]
        public void FalenLeaderBehavTest_1()// al final se convierte en jefe
        {
            falenLeader = BehaviorsLib.FallenLeader;
            

            s2.AddLayer(fallenL);
            s3.AddLayer(fallenL);
            s2.Stats.MicroService.ChangeLeader( "S1");

            //2do if sin convertirse en lider
            env.SubsribeEvent(10, p2);
            env.SubsribeEvent(18, p3);

            //1er if reiniciando los valores
            env.SubsribeEvent(34, p1);
            //env.Run();
            //Assert.AreEqual(s1.ID, s2.Stats.MicroService.LeaderId);


            //al final se convierte en jefe
            env.SubsribeEvent(42, p4);
            env.SubsribeEvent(56, p5);
            env.SubsribeEvent(67, p5);
            env.SubsribeEvent(110, p6);

            env.Run();

            Assert.AreEqual(s2.ID, s2.Stats.MicroService.LeaderId);

        }

        [TestMethod]
        public void FalenLeaderBehavTest_2() //enviando Ping Request
        {
            s2.AddLayer(fallenL);
            
            s2.Stats.MicroService.ChangeLeader("S1");

            // else de mandar el request de tipo ping
            env.SubsribeEvent(10, p2);
            env.SubsribeEvent(18, p3);

            env.Run();
            Assert.AreEqual(2, s2.Layers()[0].behaviors[0].CountPing());

        }

        [TestMethod]
        public void FalenLeaderBehavTest_3()
        {
            s2.AddLayer(fallenL);
            s3.AddLayer(fallenL2);

            s2.Stats.MicroService.ChangeLeader("S1");
            s3.Stats.MicroService.ChangeLeader("S1");


            // else de mandar el request de tipo ping
            env.SubsribeEvent(10, p2);
            env.SubsribeEvent(18, p2_1);
            env.SubsribeEvent(42, p4);
            env.SubsribeEvent(56, p4_1);
            env.SubsribeEvent(67, p5);
            env.SubsribeEvent(110, p5_1);

            env.SubsribeEvent(120, p6_1);
            env.SubsribeEvent(121, p6);

            env.Run();
           Assert.AreEqual(s3.ID, s2.Stats.MicroService.LeaderId);
           Assert.AreEqual(s3.ID, s1.Stats.MicroService.LeaderId);

        }

        [TestMethod]
        public void FalenLeaderBehavTest_4()// El jefe aparecio se reinician los valores
        {

            s2.AddLayer(fallenL);
            s3.AddLayer(fallenL);
            s2.Stats.MicroService.ChangeLeader("S1");

            //2do if sin convertirse en lider
            env.SubsribeEvent(10, p2);
            env.SubsribeEvent(18, p3);

            //1er if reiniciando los valores
            env.SubsribeEvent(34, p1);
            env.Run();
            Assert.AreEqual(env.currentTime, s2.Layers()[0].behaviors[0].LastTSeeLeader());
        }

        #endregion

        #region Decision de behavior

        [TestMethod]
        public void BehaviorSelector()
        {
            Func<IEnumerable<Behavior>, int> behaviorSelector = index => FuncionTest(listBehavior);
            fallenL.SetBehaviourSelector(behaviorSelector);

            s2.AddLayer(fallenL);
            s2.Stats.MicroService.ChangeLeader("S1");

            s2.Stats.MicroService.ForAllBiography();

            env.SubsribeEvent(10, p2);

            env.Run();

            Assert.AreEqual(0.01, s2.Stats.MicroService.Reputation(s2.ID));
            Assert.AreEqual(1, s2.Stats.CountMessagingHistory());

        }

        public int FuncionTest(List<Behavior>b)
        {
            Random r = new Random();
            return r.Next(b.Count-1);
        }
        #endregion
    }
}
