using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;
using Utils;
namespace Core {

    [TestClass]
    public class DecisorTests {
        
        #region  vars
     
        #endregion

        [TestInitialize]
        public void Init() {
            var env = new Env();

            var r1 = new Resource("img1");
            var r2 = new Resource("icon1");

            var r3 = new Resource("about.html");
            var r4 = new Resource("table1");

            var r5 = new Resource("text(caption)");
            r5.AddReqList(new List<Resource> { r1, r2 });

            var r6 = new Resource("index.html");
            r6.AddReqList(new List<Resource>{ 
                Resource.Resources["table1"],
                Resource.Resources["text(caption)"],
             });
            var r7 = new Resource("sells.html");
            
            var s1 = new Server("S1");
            var s2 = new Server("S2");
            var s3 = new Server("S3");
            var s7 = new Server("S7");
            var s8 = new Server("S8");

            var s4 = new Server("S4");
            var mPagos = new MicroService("API-Pagos");

            var s5 = new Server("S5");
            var mMulta = new MicroService("API-Multa");
            var s6 = new Server("S6");

            s1.Stats.AvailableResources = new List<Resource> { r1 };
            s2.Stats.AvailableResources = new List<Resource> { r1, r2 };
            s3.Stats.AvailableResources = new List<Resource> { r1, r2, r3 };
            s4.Stats.AvailableResources = new List<Resource> { r4, r5 };
            s5.Stats.AvailableResources = new List<Resource> { r6, r7 };
            s6.Stats.AvailableResources = new List<Resource> { r6, r7 };

            var servers = new List<Server> { s1, s2, s3, s4, s7, s8 };

            s4.SetMService(mPagos.Name);
            s5.SetMService(mMulta.Name);
            s6.SetMService(mMulta.Name);
            env.AddServerList(servers);
        }
        [TestCleanup]
        public void Clean() {
            MicroService.Services.Clear();
            Resource.Resources.Clear();
            Env.ClearServersLayers();
        }
        
        [TestMethod]
        public void DecisorInLayerTest(){
            
            var layer = new Layer();
            layer.behaviors = new List<Behavior>(){ BehaviorsLib.Worker, BehaviorsLib.Contractor};
            var s1 = Env.CurrentEnv.GetServerByID("S1");
            s1.AddLayer(layer);
            var d = new Decisor(s1.FirstLayer);
            List<Response> listResp = new List<Response>(){ 
                new Response(5, "S1", "S2", ReqType.DoIt, new Dictionary<string, bool>()),
             };
            
            var main = MicroService.GetMS("Main");
            
            d.BehaviorDecisor();

            for(int i=0;i<10;i++) {
                main.SetReward(listResp);
                Env.CurrentEnv.AdvanceTime(510);
                d.BehaviorDecisor();
            }

            Env.CurrentEnv.AdvanceTime(4000);

            int times = 10;
            while (times --> 0){
                Env.CurrentEnv.AdvanceTime(510);
                var ind = d.BehaviorDecisor();
                if (ind == 1){
                    for(int i=0;i<5;i++) main.SetReward(listResp);
                }
            }

            Assert.AreEqual(1, d.BehaviorDecisor());

            main.SetReward(listResp);
            main.SetReward(listResp);
            d.BehaviorDecisor();
        }
        

    }
}
